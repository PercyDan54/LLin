using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.IGPlayer.Feature.Player.Interfaces;
using osu.Game.Rulesets.IGPlayer.Feature.Player.Plugins.Bundle.CloudMusic.Config;
using osu.Game.Rulesets.IGPlayer.Feature.Player.Plugins.Bundle.CloudMusic.Helper;
using osu.Game.Rulesets.IGPlayer.Localisation.LLin.Plugins;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.IGPlayer.Feature.Player.Plugins.Bundle.CloudMusic.Sidebar.Graphic
{
    public partial class Toolbox : CompositeDrawable
    {
        private readonly FillFlowContainer buttonFillFlow;
        private readonly OsuSpriteText idText;

        private readonly IconButton backButton = new IconButton
        {
            Icon = FontAwesome.Solid.ArrowLeft,
            Size = new Vector2(45),
            IconColour = Color4.Black,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre
        };

        private readonly FillFlowContainer contentFillFlow = null!;
        private OsuTextBox textBox = null!;

        [Resolved]
        private LyricPlugin plugin { get; set; } = null!;

        private UserDefinitionHelper? udh;
        private readonly OsuSpriteText statusText;

        public Toolbox()
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 5;

            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;

            Margin = new MarginPadding(10);

            InternalChildren = new Drawable[]
            {
                contentFillFlow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Padding = new MarginPadding(10),
                    Children = new Drawable[]
                    {
                        idText = new OsuSpriteText
                        {
                            Margin = new MarginPadding { Horizontal = 15, Top = 15 },
                            Font = OsuFont.GetFont(size: 20),
                            Alpha = 0
                        },
                        new TrackTimeIndicator(),
                        statusText = new OsuSpriteText
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight
                        },
                        buttonFillFlow = new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Spacing = new Vector2(5),
                            AutoSizeDuration = 200,
                            AutoSizeEasing = Easing.OutQuint,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight
                        }
                    }
                }
            };
        }

        public Action? OnBackAction { get; set; }

        public string IdText
        {
            set => idText.Text = value;
        }

        public void AddButtonRange(IconButton[] range, bool isRootScreen)
        {
            buttonFillFlow.Clear(false);

            foreach (var btn in range)
            {
                btn.Anchor = btn.Origin = Anchor.Centre;

                buttonFillFlow.Add(btn);
            }

            if (!isRootScreen)
            {
                buttonFillFlow.Add(backButton);
                backButton.Action = OnBackAction;
            }
        }

        [Resolved]
        private IImplementLLin llin { get; set; }

        [Resolved]
        private LyricConfigManager lcm { get; set; }

        [BackgroundDependencyLoader]
        private void load(LyricPlugin plugin)
        {
            udh ??= plugin.UserDefinitionHelper;

            plugin.LyricProcessor.State.BindValueChanged(v =>
            {
                this.Schedule(() =>
                {
                    statusText.Text = v.NewValue.GetLocalisableDescription();

                    var color = Color4.White;

                    if (v.NewValue == LyricProcessor.SearchState.Success)
                        color = Color4.GreenYellow;
                    else if (v.NewValue == LyricProcessor.SearchState.Fail)
                        color = Color4.Gold;

                    statusText.FadeColour(color, 300, Easing.OutQuint);
                });
            }, true);

            if (LyricPlugin.DisableCloudLookup)
                initToolboxNoContent();
            else
                initToolbox();
        }

        private void initToolboxNoContent()
        {
            buttonFillFlow.Scale = new Vector2(0);

            contentFillFlow.Add(
                new OsuSpriteText
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Colour = Color4.Gold,
                    Text = "在线查询功能暂时不可用，我们正在寻找新的适配方案"
                });

            contentFillFlow.Add(new SettingsSlider<double>
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Current = plugin.Offset,
                LabelText = CloudMusicStrings.LocalOffset,
                RelativeSizeAxes = Axes.X,
                Padding = new MarginPadding { Right = 10 }
            });
        }

        private void initToolbox()
        {
            contentFillFlow.AddRange(new Drawable[]
            {
                new SettingsSlider<double>
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Current = plugin.Offset,
                    LabelText = CloudMusicStrings.LocalOffset,
                    KeyboardStep = 100,
                    RelativeSizeAxes = Axes.X,
                    Padding = new MarginPadding { Right = 10 }
                },
                textBox = new OsuTextBox
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.X,
                    PlaceholderText = CloudMusicStrings.SearchById
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new IconButton
                        {
                            Height = 30,
                            Width = 30,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            TooltipText = "更新定义",
                            Action = () =>
                            {
                                udh.UpdateDefinition(onFail: e =>
                                {
                                    Logging.Log("用户定义更新失败，请检查网络环境：" + e.Message);
                                });

                                if (lcm.Get<bool>(LyricSettings.OutputDefinitionInLogs))
                                    udh.Debug();
                            },
                            Icon = FontAwesome.Solid.Cloud
                        },
                        new IconButton
                        {
                            Height = 30,
                            Width = 30,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            TooltipText = "复制谱面参考信息",
                            Action = () =>
                            {
                                SDL2.SDL.SDL_SetClipboardText(resolveBeatmapVerboseString(plugin.CurrentWorkingBeatmap));
                                llin.PostNotification(plugin, FontAwesome.Regular.CheckCircle, "复制成功！");
                            },
                            Icon = FontAwesome.Solid.Clipboard
                        }
                    }
                }
            });

            textBox.OnCommit += (sender, isNewText) =>
            {
                if (int.TryParse(sender.Text, out int id))
                    plugin.GetLyricFor(id);
                else
                {
                    textBox.Text = string.Empty;
                }
            };
        }

        private string resolveBeatmapVerboseString(WorkingBeatmap working)
        {
            return $"{working.BeatmapSetInfo.OnlineID},"
                   + $" // Title: '{working.Metadata.TitleUnicode}'"
                   + $"('{working.Metadata.Title}')"
                   + $" Artist: '{working.Metadata.ArtistUnicode}'"
                   + $"('{working.Metadata.Artist}')"
                   + $" Source: '{working.Metadata.Source}'";
        }
    }
}
