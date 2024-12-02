<lane vertical-content-alignment="middle">
    <panel>
        <image layout="stretch"
               fit="stretch"
               sprite={@Mods/focustense.FishinC/Sprites/Cursors:OverlayFrame}
               tint="#96c8f0" />
        <image layout="64px" margin="17" sprite={FishData} />
    </panel>
    <panel *if={HasCatchesRemaining}>
        <image layout="stretch"
               fit="stretch"
               sprite={@Mods/focustense.FishinC/Sprites/Cursors:OverlayDrawer}
               tint="#96c8f0" />
        <lane margin="9, 25, 30, 18" vertical-content-alignment="middle">
            <label margin="0, 0, 4, 0" max-lines="1" text={CatchesRemaining} />
            <image sprite={@Mods/focustense.FishinC/Sprites/Cursors:GenericFish} />
        </lane>
    </panel>
</lane>