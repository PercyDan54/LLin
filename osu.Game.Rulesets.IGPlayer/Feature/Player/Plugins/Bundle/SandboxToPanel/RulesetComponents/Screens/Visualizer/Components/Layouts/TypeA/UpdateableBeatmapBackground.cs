﻿#nullable disable

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.IGPlayer.Feature.Player.Misc;
using osu.Game.Rulesets.IGPlayer.Feature.Player.Plugins.Bundle.SandboxToPanel.RulesetComponents.Configuration;
using osu.Game.Rulesets.IGPlayer.Feature.Player.Plugins.Bundle.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.MusicHelpers;
using osuTK;
using osuTK.Graphics;
using BeatmapBackground = osu.Game.Graphics.Backgrounds.BeatmapBackground;

namespace osu.Game.Rulesets.IGPlayer.Feature.Player.Plugins.Bundle.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Layouts.TypeA
{
    public partial class UpdateableBeatmapBackground : CurrentBeatmapProvider
    {
        private const int animation_duration = 500;

        private readonly Container backgroundContainer;
        private readonly Container nameContainer;
        private readonly MusicIntensityController intensityController;

        private BeatmapBackground background;
        private BeatmapName name;

        private Container bgContainerWrapper;

        public UpdateableBeatmapBackground()
        {
            AddRangeInternal(new Drawable[]
            {
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        bgContainerWrapper = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Child =
                                backgroundContainer = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Size = new Vector2(1.2f),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                },
                        },
                        nameContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                    }
                },
                intensityController = new MusicIntensityController()
            });
        }

        private readonly BindableBool spinningWrapper = new BindableBool();

        [BackgroundDependencyLoader]
        private void load(SandboxRulesetConfigManager rulesetConfigManager)
        {
            rulesetConfigManager.BindWith(SandboxRulesetSetting.SpinningCoverAndVisualizer, spinningWrapper);
            spinningWrapper.BindValueChanged(v =>
            {
                if (v.NewValue)
                {
                    float rotation = bgContainerWrapper.Rotation;
                    bgContainerWrapper.RotateTo(rotation).Then().RotateTo(360 + rotation, 30000).Loop();
                }
                else
                    bgContainerWrapper.RotateTo(0, 1500, Easing.OutBack);
            }, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            intensityController.Intensity.BindValueChanged(intensity =>
            {
                var adjustedIntensity = intensity.NewValue / 150;

                if (adjustedIntensity > 0.2f)
                    adjustedIntensity = 0.2f;

                var sizeDelta = 1.2f - adjustedIntensity;

                if (sizeDelta > backgroundContainer.Size.X)
                    return;

                backgroundContainer.ResizeTo(sizeDelta, 10, Easing.OutQuint).Then().ResizeTo(1.2f, 1500, Easing.OutQuint);
            }, true);
        }

        protected override void OnBeatmapChanged(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
            LoadComponentAsync(new BeatmapBackground(beatmap.NewValue)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0,
                Colour = Color4.DarkGray
            }, newBackground =>
            {
                background?.FadeOut(animation_duration, Easing.OutQuint);
                background?.RotateTo(360, animation_duration, Easing.OutQuint);
                background?.Expire();

                background = newBackground;
                backgroundContainer.Add(newBackground);
                newBackground.RotateTo(360, animation_duration, Easing.OutQuint);
                newBackground.FadeIn(animation_duration, Easing.OutQuint);
            });

            LoadComponentAsync(new BeatmapName(beatmap.NewValue)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Y = -1.2f,
                Depth = -float.MaxValue,
            }, newName =>
            {
                name?.MoveToY(1.2f, animation_duration, Easing.Out);
                name?.Expire();

                name = newName;
                nameContainer.Add(newName);
                newName.MoveToY(0, animation_duration, Easing.OutQuint);
            });
        }

        private partial class BeatmapName : CompositeDrawable
        {
            [Resolved(canBeNull: true)]
            private SandboxRulesetConfigManager config { get; set; }

            private readonly Bindable<int> radius = new Bindable<int>(350);
            private readonly Bindable<string> colour = new Bindable<string>("#ffffff");

            private readonly WorkingBeatmap beatmap;
            private TextFlowContainer artist;
            private TextFlowContainer title;
            private FillFlowContainer fillFlow;
            private BufferedContainer bufferedContainer;

            public BeatmapName(WorkingBeatmap beatmap = null)
            {
                this.beatmap = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;
                RelativePositionAxes = Axes.Y;
                Padding = new MarginPadding { Horizontal = 30 };

                if (beatmap == null)
                    return;

                fillFlow = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 10),
                    Children = new Drawable[]
                    {
                        title = new TextFlowContainer(t =>
                        {
                            t.Font = OsuFont.GetFont(size: 28, weight: FontWeight.SemiBold);
                        })
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            TextAnchor = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = beatmap.Metadata.GetTitleRomanisable()
                        },
                        artist = new TextFlowContainer(t =>
                        {
                            t.Font = OsuFont.GetFont(size: 22, weight: FontWeight.Bold);
                        })
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            TextAnchor = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = beatmap.Metadata.GetArtistRomanisable()
                        }
                    }
                };

                bufferedContainer = fillFlow.WithEffect(new BlurEffect
                {
                    Colour = Color4.Black.Opacity(0.7f),
                    DrawOriginal = true,
                    PadExtent = true,
                    Sigma = new Vector2(5)
                });

                AddInternal(bufferedContainer);

                config?.BindWith(SandboxRulesetSetting.Radius, radius);
                config?.BindWith(SandboxRulesetSetting.TypeATextColour, colour);
            }

            protected override void UpdateAfterChildren()
            {
                llin_workaroundForFillflowHeight();

                base.UpdateAfterChildren();
            }

            private void llin_workaroundForFillflowHeight()
            {
                float expectedHeight = fillFlow.Children.Sum(fillFlowChild => fillFlowChild.Height);
                expectedHeight += fillFlow.Spacing.Y * (fillFlow.Children.Count - 1);

                float diff = fillFlow.Height - expectedHeight;

                bufferedContainer.Y = diff / 2;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                radius.BindValueChanged(r =>
                {
                    if (beatmap != null)
                        Scale = new Vector2(r.NewValue / 350f);
                }, true);

                colour.BindValueChanged(c => artist.Colour = title.Colour = Colour4.FromHex(c.NewValue), true);
            }
        }
    }
}
