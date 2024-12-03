<lane orientation="vertical">
    <panel vertical-content-alignment="middle">
       <image layout="stretch stretch[60..]"
              fit="stretch"
              sprite={@Mods/focustense.FishinC/Sprites/Cursors:SpeechBubbleBody} />
        <panel margin="12" padding="4">
            <lane orientation="vertical" horizontal-content-alignment="middle">
                <panel layout="80px"
                       horizontal-content-alignment="start"
                       vertical-content-alignment="end">
                    <image *repeat={Fish}
                           layout="64px"
                           margin={:Margin}
                           sprite={:^FishSprite}
                           tint={:Tint}
                           shadow-alpha="0.35"
                           shadow-offset="-2, 2" />
                </panel>
                <lane margin="0, 8, 0, 0" vertical-content-alignment="middle">
                    <image layout="36px"
                           margin="0, 0, 8, 0"
                           sprite={@Mods/focustense.FishinC/Sprites/Cursors:TinyClock}
                           shadow-alpha="0.5"
                           shadow-offset="-2, 2" />
                    <label font="dialogue" bold="true" color={DurationColor} text={DurationText} />
                </lane>
            </lane>
       </panel>
    </panel>
    <image layout="stretch 4px"
           fit="stretch"
           margin="20, -4, 20, 0"
           sprite={@Mods/focustense.FishinC/Sprites/Cursors:SpeechBubbleCloser} />
    <image layout="stretch content"
           margin="0, -4, 0, 0"
           fit="none"
           horizontal-alignment="middle"
           sprite={@Mods/focustense.FishinC/Sprites/Cursors:SpeechBubbleTail} />
</lane>