<lane orientation="vertical">
    <frame background={@Mods/StardewUI/Sprites/MenuBackground}
           border={@Mods/StardewUI/Sprites/MenuBorder}
           border-thickness="36, 36, 40, 36">
        <lane layout="800px stretch[..1200]" orientation="vertical">
            <panel *context={:Header}
                   layout="stretch 150px"
                   horizontal-content-alignment="middle"
                   vertical-content-alignment="middle">
                <image layout="stretch"
                       fit="cover"
                       sprite={@Mods/focustense.FishinC/Sprites/Cursors:Water} />
                <lane layout="stretch" vertical-content-alignment="middle">
                    <image layout="128px" margin="32, 0, 0, 0" sprite={GiantBait} />
                    <banner layout="stretch content"
                            text={#Settings.ModTitle}
                            text-shadow-alpha="0.6"
                            text-shadow-offset="-4, 4" />
                    <image layout="128px" margin="0, 0, 24, 0" sprite={Mermaid} />
                </lane>
            </panel>
            <image layout="stretch content"
                   fit="stretch"
                   margin="-36, -20, -40, -20"
                   sprite={@Mods/StardewUI/Sprites/MenuHorizontalDivider} />
            <scrollable layout="stretch" peeking="128">
                <lane layout="stretch content" padding="8" orientation="vertical">
                    <label layout="stretch content" color="#404040" text={:Quote} />
                    <form-heading title={#Settings.Difficulty.Heading} />
                    <lane margin="16, 0, 0, 0">
                        <rule-set-button *repeat={:StandardRuleSets}
                                         name={:Name}
                                         description={:Description}
                                         title={:Title}
                                         sprite={:SpriteData}
                                         selected={<>IsSelected} />
                        <rule-set-button *context={:CustomRuleSet}
                                         name=""
                                         title={#Settings.Difficulty.Custom.Title}
                                         description={#Settings.Difficulty.Custom.Description}
                                         sprite={@Item/(O)128}
                                         selected={<>^IsCustomRuleSetSelected} />
                    </lane>
                    <frame *switch={SelectedRuleSetType} layout="stretch content">
                        <lane *case="standard"
                              *context={SelectedStandardRuleSet}
                              layout="stretch content"
                              margin="16, 16, 0, 0"
                              orientation="vertical">
                            <rule-summary *repeat={:Rules} text={:this} />
                        </lane>
                        <lane *case="custom"
                              *context={CustomRuleSet}
                              layout="stretch content"
                              margin="0, 16, 0, 0"
                              orientation="vertical">
                            <form-row title={#Settings.Rules.CurrentBubbles.Title}
                                      description={#Settings.Rules.CurrentBubbles.Description}>
                                <condition-dropdown value={<>CurrentBubblesCondition} />
                            </form-row>
                            <form-row title={#Settings.Rules.FutureBubbles.Title}
                                      description={#Settings.Rules.FutureBubbles.Description}>
                                <condition-dropdown value={<>FutureBubblesCondition} />
                            </form-row>
                            <form-row title={#Settings.Rules.FishPredictions.Title}
                                      description={#Settings.Rules.FishPredictions.Description}>
                                <condition-dropdown value={<>FishPredictionsCondition} />
                            </form-row>
                            <form-row title={#Settings.Rules.JellyPredictions.Title}
                                      description={#Settings.Rules.JellyPredictions.Description}>
                                <condition-dropdown value={<>JellyPredictionsCondition} />
                            </form-row>
                            <form-row title={#Settings.Rules.FreezeOnCast.Title}
                                      description={#Settings.Rules.FreezeOnCast.Description}>
                                <checkbox is-checked={<>FreezeOnCast} />
                            </form-row>
                            <form-row title={#Settings.Rules.RespawnOnCancel.Title}
                                      description={#Settings.Rules.RespawnOnCancel.Description}>
                                <checkbox is-checked={<>RespawnOnCancel} />
                            </form-row>
                        </lane>
                    </frame>
                    <form-heading title={#Settings.Time.Heading} />
                    <form-row title={#Settings.Time.FishingSpeedup.Title}
                              description={#Settings.Time.FishingSpeedup.Description}>
                        <slider track-width="300"
                                min="1"
                                max="10"
                                interval="1"
                                value={<>FishingTimeScale}
                                value-color="#404040"
                                value-format={:FishingTimeScaleFormat} />
                    </form-row>
                    <form-row title={#Settings.Time.RerollInterval.Title}
                              description={#Settings.Time.RerollInterval.Description}>
                        <slider track-width="300"
                                min="10"
                                max="300"
                                interval="10"
                                value={<>RespawnInterval}
                                value-color="#404040"
                                value-format={:RespawnIntervalFormat} />
                    </form-row>
                    <form-heading title={#Settings.UI.Heading} />
                    <form-row title={#Settings.UI.PreviewEnableOnLoad.Title}
                              description={#Settings.UI.PreviewEnableOnLoad.Description}>
                        <checkbox is-checked={<>EnablePreviewsOnLoad} />
                    </form-row>
                    <form-row title={#Settings.UI.PreviewKeybind.Title}
                              description={#Settings.UI.PreviewKeybind.Description}>
                        <keybind-editor button-height="48"
                                        sprite-map={@Mods/StardewUI/SpriteMaps/Buttons:default-default-0.3}
                                        editable-type="MultipleKeybinds"
                                        font="small"
                                        add-button-text={#Settings.UI.PreviewKeybind.Editor.Add.Text}
                                        delete-button-tooltip={#Settings.UI.PreviewKeybind.Editor.Delete.Description}
                                        empty-text={#Settings.UI.PreviewKeybind.Empty}
                                        empty-text-color="#404040"
                                        focusable="true"
                                        keybind-list={<>CatchPreviewToggleKeybinds} />
                    </form-row>
                    <form-row title={#Settings.UI.CatchPreviewRadius.Title}
                              description={#Settings.UI.CatchPreviewRadius.Description}>
                        <slider track-width="300"
                                min="2"
                                max="20"
                                interval="1"
                                value={<>PreviewRadius}
                                value-color="#404040"
                                value-format={:PreviewRadiusFormat} />
                    </form-row>
                    <form-row title={#Settings.UI.HudLocation.Title}
                              description={#Settings.UI.HudLocation.Description}>
                        <nine-grid-editor layout="80px"
                                          hover-tint-color="orange"
                                          button-sprite-map={@Mods/StardewUI/SpriteMaps/Buttons:dark-light}
                                          direction-sprite-map={@Mods/StardewUI/SpriteMaps/Directions}
                                          placement={<>SeededRandomFishHudPlacement}
                                          focusable="true">
                            <include *context={:ExampleSeedFishInfo}
                                     name="Mods/focustense.FishinC/Views/SeedFishInfo" />
                        </nine-grid-editor>
                    </form-row>
                </lane>
            </scrollable>
        </lane>
    </frame>
    <lane layout="stretch content"
          margin="0, -12, 20, 0"
          horizontal-content-alignment="end"
          vertical-content-alignment="middle">
        <action-button text={#Settings.Button.Default} action="reset" />
        <action-button text={#Settings.Button.Cancel} action="cancel" />
        <action-button text={#Settings.Button.Save} action="save" />
    </lane>
</lane>

<template name="rule-set-button">
    <frame margin="0, 0, 32, 0"
           padding="4"
           background={@Mods/StardewUI/Sprites/MenuSlotInset}
           focusable="true"
           tooltip={&description}
           left-click=|^SelectRuleSet(&name)|>
        <panel layout="content[150..] content"
               horizontal-content-alignment="middle">
            <image *if={&selected}
                   layout="stretch"
                   fit="stretch"
                   sprite={@Mods/StardewUI/Sprites/White}
                   tint="LightBlue" />
            <lane margin="12"
                  orientation="vertical"
                  horizontal-content-alignment="middle">
                <image layout="80px" margin="0, 0, 0, 8" sprite={&sprite} />
                <label text={&title} />
            </lane>
        </panel>
    </frame>
</template>

<template name="rule-summary">
    <lane layout="stretch content" margin="0, 0, 0, 8" vertical-content-alignment="middle">
        <image layout="21px 24px"
               margin="0, 0, 8, 0"
               sprite={@Mods/focustense.FishinC/Sprites/Cursors:BobberDefault}
               tint="#cccc" />
        <label layout="stretch content" color="#404040" text={&text} />
    </lane>
</template>

<template name="form-heading">
    <banner margin="0, 16, 0, 0" text={&title} />
</template>

<template name="form-row">
    <lane layout="stretch content"
          margin="16, 8, 0, 8"
          vertical-content-alignment="middle"
          scroll-with-children="vertical">
        <label layout="300px content"
               text={&title}
               tooltip={&description}
               focusable="true" />
        <outlet />
    </lane>
</template>

<template name="condition-dropdown">
    <dropdown options={:ConditionKeys}
              option-format={:ConditionFormat}
              option-min-width="250"
              selected-option={&value} />
</template>

<template name="action-button">
    <button layout="content[150..] content"
            margin="8, 0, 0, 0"
            font="dialogue"
            default-background={@Mods/StardewUI/Sprites/ButtonDark}
            hover-background={@Mods/StardewUI/Sprites/ButtonLight}
            shadow-visible="true"
            text={&text}
            left-click=|PerformAction(&action)| />
</template>