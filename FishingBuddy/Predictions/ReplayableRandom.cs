using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewModdingAPI.Utilities;

namespace FishingBuddy.Predictions;

/// <summary>
/// <see cref="Random"/> implementation that derives from another random source, generally global
/// entropy, and can be reset to replay a previous random sequence.
/// </summary>
internal class ReplayableRandom : Random
{
    /// <summary>
    /// Replayable fork of the game's global entropy (<see cref="Game1.random"/>).
    /// </summary>
    public static ReplayableRandom Global => global.Value;

    private object Impl
    {
        get => Traverse.Create(this).Field("_impl").GetValue();
        set => Traverse.Create(this).Field("_impl").SetValue(value);
    }

    private static readonly PerScreen<ReplayableRandom> global = new(() => new(() => Game1.random));

    private readonly Func<Random> sourceFactory;

    private IPrngState? snapshot;
    private Random? source;

    /// <summary>
    /// Creates a new <see cref="ReplayableRandom"/> using a factory/selector providing the source
    /// instance from which to fork.
    /// </summary>
    /// <param name="sourceFactory">Factory for the random source.</param>
    public ReplayableRandom(Func<Random> sourceFactory)
    {
        this.sourceFactory = sourceFactory;
        Snapshot();
    }

    /// <summary>
    /// Reverts the state to the most recent <see cref="Snapshot"/>, causing all random samples
    /// to replay the same values seen previously.
    /// </summary>
    /// <returns><c>true</c> if successfully rewound; <c>false</c> if something went wrong with the
    /// underlying <see cref="Random"/> implementations and rewind was unsuccessful.</returns>
    public bool Rewind()
    {
        if (snapshot is null)
        {
            return false;
        }
        return snapshot.TryWrite(Impl);
    }

    /// <summary>
    /// Takes a snapshot of the current random state, so that the next <see cref="Rewind"/> will
    /// return to this state.
    /// </summary>
    /// <returns><c>true</c> if the snapshot was taken successfully; <c>false</c> if something went
    /// wrong with the underlying <see cref="Random"/> implementations and the snapshot could not be
    /// obtained.</returns>
    public bool Snapshot()
    {
        if (!TryUpdateSource())
        {
            return false;
        }
        var sourceImpl = Traverse.Create(source).Field("_impl").GetValue();
        snapshot = CreatePrngState(sourceImpl);
        return snapshot is not null && snapshot.TryRead(sourceImpl) && snapshot.TryWrite(Impl);
    }

    private static IPrngState? CreatePrngState(object impl)
    {
        return impl.GetType().Name switch
        {
            "XoshiroImpl" => XoshiroState.Create(impl),
            "Net5CompatSeedImpl" or "Net5CompatDerivedImpl" => new CompatPrngState(),
            _ => null,
        };
    }

    private static object? CreateSameImplType(object sourceImpl)
    {
        var implType = sourceImpl.GetType();
        switch (implType.Name)
        {
            case "XoshiroImpl":
                return Activator.CreateInstance(implType);
            case "Net5CompatSeedImpl":
                return Activator.CreateInstance(implType, 0);
            case "Net5CompatDerivedImpl":
                var parent = Traverse.Create(sourceImpl).Field("_parent").GetValue();
                return Activator.CreateInstance(implType, parent, 0);
            default:
                return null;
        }
    }

    [MemberNotNullWhen(true, nameof(source))]
    private bool TryUpdateSource()
    {
        var nextSource = sourceFactory();
        if (nextSource == source)
        {
            return true;
        }
        source = nextSource;
        var nextImpl = Traverse.Create(nextSource).Field("_impl").GetValue();
        var thisImpl = CreateSameImplType(nextImpl);
        if (thisImpl is null)
        {
            return false;
        }
        Impl = thisImpl;
        return true;
    }
}

interface IPrngState
{
    bool TryRead(object randomImpl);
    bool TryWrite(object randomImpl);
}

class CompatPrngState : IPrngState
{
    private int[]? seedArray;
    private int inext;
    private int inextp;

    public bool TryRead(object randomImpl)
    {
        var implTraverse = Traverse.Create(randomImpl);
        var prngTraverse = implTraverse.Field("_prng");
        // Net5CompatDerivedImpl lazily initializes the seedArray on random samples, so in order
        // to read its state we must force it to initialize.
        ///
        // It doesn't seem like the derived impl is actually used anywhere in Stardew, and even if
        // it were, this probably wouldn't be reliable because the parent holds its own state too.
        // But hey, we'll give it the old college try.
        var seedField = implTraverse.Field("_seed");
        if (seedField.FieldExists())
        {
            prngTraverse.Method("EnsureInitialized").GetValue(seedField.GetValue());
        }
        var seedArray = prngTraverse.Field("_seedArray").GetValue<int[]>();
        if (seedArray is null)
        {
            this.seedArray = null;
            return false;
        }
        else
        {
            if (this.seedArray is null || this.seedArray.Length != seedArray.Length)
            {
                this.seedArray = new int[seedArray.Length];
            }
            Array.Copy(seedArray, this.seedArray, seedArray.Length);
        }
        inext = prngTraverse.Field("_inext").GetValue<int>();
        inextp = prngTraverse.Field("_inextp").GetValue<int>();
        return true;
    }

    public bool TryWrite(object randomImpl)
    {
        if (this.seedArray is null)
        {
            return false;
        }
        // CompatPrng is a mutable struct and Harmony doesn't (apparently) know how to work around
        // the implicit copying/boxing/unboxing issues. So we have to do this in separate steps:
        // Read the entire struct (copying it), write fields of the struct, then write the struct.
        var prngField = Traverse.Create(randomImpl).Field("_prng");
        var prng = prngField.GetValue();
        var prngTraverse = Traverse.Create(prng);
        var seedArray = prngTraverse.Field("_seedArray").GetValue<int[]?>();
        if (seedArray is null || seedArray.Length != this.seedArray.Length)
        {
            seedArray = new int[this.seedArray.Length];
            prngTraverse.Field("_seedArray").SetValue(seedArray);
        }
        Array.Copy(this.seedArray, seedArray, this.seedArray.Length);
        prngTraverse.Field("_inext").SetValue(inext);
        prngTraverse.Field("_inextp").SetValue(inextp);
        prngField.SetValue(prng);
        return true;
    }
}

static class XoshiroState
{
    public static IPrngState? Create(object sourceImpl)
    {
        var fieldType = Traverse.Create(sourceImpl).Field("_s0").GetValueType();
        if (fieldType == typeof(uint))
        {
            return new XoshiroState<uint>();
        }
        if (fieldType == typeof(ulong))
        {
            return new XoshiroState<ulong>();
        }
        return null;
    }
}

class XoshiroState<T> : IPrngState
    where T : unmanaged
{
    private T s0;
    private T s1;
    private T s2;
    private T s3;

    public bool TryRead(object randomImpl)
    {
        var traverse = Traverse.Create(randomImpl);
        s0 = traverse.Field("_s0").GetValue<T>();
        s1 = traverse.Field("_s1").GetValue<T>();
        s2 = traverse.Field("_s2").GetValue<T>();
        s3 = traverse.Field("_s3").GetValue<T>();
        return true;
    }

    public bool TryWrite(object randomImpl)
    {
        var traverse = Traverse.Create(randomImpl);
        traverse.Field("_s0").SetValue(s0);
        traverse.Field("_s1").SetValue(s1);
        traverse.Field("_s2").SetValue(s2);
        traverse.Field("_s3").SetValue(s3);
        return true;
    }
}
