using System.Collections.Generic;
using Lunar.Core;

namespace Lunar.Data
{
    public static class LunarDefaultConfigFactory
    {
        public static Dictionary<LunarDay, LunarDayConfig> CreateDayConfigs()
        {
            var configs = new Dictionary<LunarDay, LunarDayConfig>();

            foreach (LunarDay day in System.Enum.GetValues(typeof(LunarDay)))
            {
                configs[day] = CreateDayConfig(day);
            }

            return configs;
        }

        public static GlobalConfig CreateGlobalConfig()
        {
            var config = new GlobalConfig();
            config.audioMix.layers.Add(new AudioLayerConfig
            {
                layerName = "LowFrequencyHum",
                clipResourcePath = "Audio/ambient_low",
                volume = 0.6f,
                loop = true
            });
            config.audioMix.layers.Add(new AudioLayerConfig
            {
                layerName = "BreathGuide",
                clipResourcePath = "Audio/breath_60bpm",
                volume = 0.45f,
                loop = true
            });
            config.days.AddRange(CreateDayConfigs().Values);
            return config;
        }

        private static LunarDayConfig CreateDayConfig(LunarDay day)
        {
            var config = new LunarDayConfig
            {
                dayNumber = (int)day,
                dayName = $"Day {(int)day}: {GetDayName(day)}",
                theme = GetTheme(day),
                emotionalGoal = GetEmotionalGoal(day),
                dramaticQuestion = GetDramaticQuestion(day),
                externalPressure = GetExternalPressure(day),
                innerShift = GetInnerShift(day),
                visualMotif = GetVisualMotif(day),
                targetDurationMinutes = day == LunarDay.Day5_Ritual ? 7f : 4f,
                introductionDurationSeconds = day == LunarDay.Day1_Arrival ? 30f : 20f,
                resourceDurationSeconds = day == LunarDay.Day5_Ritual ? 80f : 110f,
                narrationDurationSeconds = day == LunarDay.Day5_Ritual ? 50f : 35f,
                enableNarration = true,
                enableRitual = true,
                hasAnomaly = day == LunarDay.Day4_Uncertainty,
                anomalyChance = day == LunarDay.Day4_Uncertainty ? 0.5f : 0f,
                ritual = CreateRitual(day)
            };

            config.narrativeClips.AddRange(CreateNarrativeClipSet(day));
            config.documentaryClips.AddRange(CreateDocumentaryClipSet(day));
            config.narrativeBeats.AddRange(CreateNarrativeBeats(day));

            config.resources.Add(CreateResourceConfig(ResourceType.Energy, 0.002f, 0.12f));
            config.resources.Add(CreateResourceConfig(ResourceType.Oxygen, 0.0012f, 0.10f));
            config.resources.Add(CreateResourceConfig(ResourceType.Water, 0.0015f, 0.10f));

            return config;
        }

        private static RitualConfig CreateRitual(LunarDay day)
        {
            var config = new RitualConfig
            {
                targetDay = day,
                ritualName = $"{GetDayName(day)} Ritual",
                description = GetRitualDescription(day),
                intention = GetRitualIntention(day),
                entryPrompt = GetRitualEntryPrompt(day),
                exitPrompt = GetRitualExitPrompt(day),
                isDeepRitual = day == LunarDay.Day5_Ritual
            };

            if (day == LunarDay.Day5_Ritual)
            {
                config.phases.Add(CreatePhase(RitualPhase.Enter, 30f, "Ritual_Enter", "把视线从面板上移开，先确认呼吸还在。"));
                config.phases.Add(CreatePhase(RitualPhase.Anchor, 90f, "Ritual_Anchor", "吸气时感受胸腔抬起，呼气时让肩膀慢下来。"));
                config.phases.Add(CreatePhase(RitualPhase.Order, 60f, "Ritual_Order", "阀门只做一件事，你也只做这一件事。", true, "valve"));
                config.phases.Add(CreatePhase(RitualPhase.Observe, 120f, "Ritual_Observe", "如果思绪回来，不要驱赶，只要知道它回来过。"));
                config.phases.Add(CreatePhase(RitualPhase.Exit, 30f, "Ritual_Exit", "当灯重新亮起，你不需要变成另一个人。"));
                return config;
            }

            config.phases.Add(CreatePhase(RitualPhase.Enter, 15f, "Ritual_Enter", GetRitualEntryPrompt(day)));
            config.phases.Add(CreatePhase(RitualPhase.Anchor, 45f, "Ritual_Anchor", GetRitualAnchorLine(day)));
            config.phases.Add(CreatePhase(RitualPhase.Observe, 60f, "Ritual_Observe", GetRitualObserveLine(day)));
            config.phases.Add(CreatePhase(RitualPhase.Exit, 15f, "Ritual_Exit", GetRitualExitPrompt(day)));
            return config;
        }

        private static RitualPhaseConfig CreatePhase(
            RitualPhase phase,
            float durationSeconds,
            string audioClipName,
            string voiceoverScript,
            bool requiresInteraction = false,
            string interactionTarget = "")
        {
            return new RitualPhaseConfig
            {
                phase = phase,
                durationSeconds = durationSeconds,
                audioClipName = audioClipName,
                voiceoverScript = voiceoverScript,
                requiresInteraction = requiresInteraction,
                interactionTarget = interactionTarget
            };
        }

        private static ResourceConfig CreateResourceConfig(ResourceType type, float decayRate, float recoveryRate)
        {
            return new ResourceConfig
            {
                type = type,
                decayRate = decayRate,
                recoveryRate = recoveryRate
            };
        }

        private static List<string> CreateNarrativeClipSet(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    return new List<string>
                    {
                        "Narrative_Day1_Arrival_LockIn",
                        "Narrative_Day1_Arrival_Inventory",
                        "Narrative_Day1_Arrival_Rest"
                    };
                case LunarDay.Day2_Adaptation:
                    return new List<string>
                    {
                        "Narrative_Day2_Adaptation_Corner",
                        "Narrative_Day2_Adaptation_NoiseMap",
                        "Narrative_Day2_Adaptation_Pause"
                    };
                case LunarDay.Day3_Order:
                    return new List<string>
                    {
                        "Narrative_Day3_Order_Checklist",
                        "Narrative_Day3_Order_Repetition",
                        "Narrative_Day3_Order_BreathGap"
                    };
                case LunarDay.Day4_Uncertainty:
                    return new List<string>
                    {
                        "Narrative_Day4_Uncertainty_Flicker",
                        "Narrative_Day4_Uncertainty_Wait",
                        "Narrative_Day4_Uncertainty_Stay"
                    };
                case LunarDay.Day5_Ritual:
                    return new List<string>
                    {
                        "Narrative_Day5_Ritual_Disarm",
                        "Narrative_Day5_Ritual_Valve",
                        "Narrative_Day5_Ritual_Return"
                    };
                case LunarDay.Day6_Stability:
                    return new List<string>
                    {
                        "Narrative_Day6_Stability_Reentry",
                        "Narrative_Day6_Stability_SofterHands",
                        "Narrative_Day6_Stability_QuietSystems"
                    };
                case LunarDay.Day7_Reflection:
                    return new List<string>
                    {
                        "Narrative_Day7_Reflection_Window",
                        "Narrative_Day7_Reflection_Earthrise",
                        "Narrative_Day7_Reflection_Carry"
                    };
                default:
                    return new List<string>();
            }
        }

        private static List<string> CreateDocumentaryClipSet(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    return new List<string> { "Archive_Apollo_CabinTone" };
                case LunarDay.Day3_Order:
                    return new List<string> { "Archive_ISS_DailyChecklist" };
                case LunarDay.Day4_Uncertainty:
                    return new List<string> { "Archive_Astronaut_SignalDelay" };
                case LunarDay.Day7_Reflection:
                    return new List<string> { "Archive_Earthrise_Reading" };
                default:
                    return new List<string>();
            }
        }

        private static List<NarrativeBeatConfig> CreateNarrativeBeats(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    return new List<NarrativeBeatConfig>
                    {
                        CreateBeat(
                            1,
                            "自我揭示、需要、欲望",
                            "day1_lockin",
                            "气闸闭合",
                            "scene_start",
                            "主角想以零事故完成单人第一周来证明自己适合长期驻月，但真正的需要是先学会让自己待在这里。",
                            "门关上以后，地球没有更远，只是声音更少了。",
                            "灯光从脉冲红缓慢过渡到稳定冷白。"),
                        CreateBeat(
                            2,
                            "鬼魂与故事世界",
                            "day1_ghost",
                            "旧伤回声",
                            "introduction_midpoint",
                            "轨道模拟任务里那次‘修好系统却没照看住人’的经历，和月球站这个低刺激世界一起被建立。",
                            "我以前以为，只要数值回到安全区，人就会跟着安全。",
                            "舱内环境音被短暂抽空，只剩呼吸和设备底噪。"),
                        CreateBeat(
                            3,
                            "弱点与需求",
                            "day1_inventory",
                            "盘点仍可用之物",
                            "first_resource_interaction",
                            "主角的弱点被确认：一紧张就会立刻盘点、修正、试图靠控制压住不安。",
                            "我先数清还能用的东西，再决定害怕什么。",
                            "资源节点从闪烁切换为常亮。"),
                        CreateBeat(
                            4,
                            "激励事件",
                            "day1_protocol",
                            "协议上线",
                            "day_transition",
                            "Caretaker Protocol 接管第一周日程，正式把主角推进这场故事。",
                            "从现在开始，每一项都要按时完成。至少系统是这么认为的。",
                            "维护清单投影亮起，第一周节律被锁定。")
                    };

                case LunarDay.Day2_Adaptation:
                    return new List<NarrativeBeatConfig>
                    {
                        CreateBeat(
                            5,
                            "欲望",
                            "day2_goal",
                            "给自己定下零事故目标",
                            "scene_start",
                            "主角把故事表层目标定义为：零事故完成第一周，证明自己能长期值守。",
                            "只要这一周足够稳，后面的事就会容易很多。",
                            "值守日志被打开，标题停在 First Solo Week。"),
                        CreateBeat(
                            6,
                            "盟友",
                            "day2_allies",
                            "前任和值守档案出现",
                            "resource_management_midpoint",
                            "前任值守员留下的异常短句、历史音频和 Quiet Deck 开始成为盟友。",
                            "有人在这里也留下过一句不属于清单的话。",
                            "静区灯光第一次以独立回路被点亮。"),
                        CreateBeat(
                            7,
                            "对手与谜团",
                            "day2_mystery",
                            "为什么这里会有仪式化痕迹",
                            "ritual_start",
                            "主角发现站内并不只是高效维护设施，似乎还藏着某种为人而设的节律设计。",
                            "这不是标准站点会留下的东西。那它为什么还在？",
                            "手动阀门附近留下非标准磨损痕迹。")
                    };

                case LunarDay.Day3_Order:
                    return new List<NarrativeBeatConfig>
                    {
                        CreateBeat(
                            8,
                            "假盟友真对手",
                            "day3_protocol_comfort",
                            "协议给出秩序的安慰",
                            "scene_start",
                            "Caretaker Protocol 看起来像高效助手，实则正在把主角重新推回过度控制。",
                            "当事情按顺序发生，我会暂时忘记自己为什么紧张。",
                            "所有维护项被排成更紧的时间块。"),
                        CreateBeat(
                            9,
                            "第一次揭示与决定",
                            "day3_breath_note",
                            "前任的呼吸间隔不是错误",
                            "resource_management_midpoint",
                            "主角第一次意识到前任日志里的间隔标记不是笔误，而是人为节律。",
                            "也许他不是在偷懒。也许他是在把自己留在这里。",
                            "日志上出现重复的四拍标记。"),
                        CreateBeat(
                            10,
                            "计划",
                            "day3_plan",
                            "决定把节律嵌入维护",
                            "ritual_start",
                            "主角决定一边照常完成维护，一边试着把呼吸和动作对齐。",
                            "如果我做不到完全不紧张，至少可以先让动作别那么急。",
                            "维护反馈节奏第一次和呼吸引导同步。")
                    };

                case LunarDay.Day4_Uncertainty:
                    return new List<NarrativeBeatConfig>
                    {
                        CreateBeat(
                            11,
                            "对手计划与第一次重击",
                            "day4_protocol_counterattack",
                            "协议开始压缩时间窗口",
                            "scene_start",
                            "对手出招：在异常前夕，协议系统增加提醒频率、压缩任务窗口，让主角更难停下。",
                            "越是接近波动，系统越要求我更快。它永远只会要更快。",
                            "清单刷新频率变高，提示音更密集。"),
                        CreateBeat(
                            12,
                            "Drive",
                            "day4_drive",
                            "用更严密的动作压住不安",
                            "resource_management_midpoint",
                            "主角没有放松，反而加倍维护，试图靠秩序反扑对抗焦虑。",
                            "如果我把每一步都做得更准，也许事情就不会滑出去。",
                            "交互速度被迫加快，环境噪音随之变尖。"),
                        CreateBeat(
                            13,
                            "盟友攻击",
                            "day4_note_attack",
                            "前任笔记戳破问题",
                            "anomaly_warning",
                            "盟友发起攻击：前任留下的私人句子直接指出主角的问题不在系统，而在无法停下。",
                            "你不是在维护站点。你是在逃避安静。",
                            "私密日志覆盖在标准工单之上。"),
                        CreateBeat(
                            14,
                            "表面失败",
                            "day4_defeat",
                            "旧方法失效",
                            "ritual_start",
                            "故障与噪点真正到来，主角靠旧方法无法迅速恢复平稳，表面失败成立。",
                            "我把每个数值都盯住了，可它们还是继续抖。",
                            "仪式区成为唯一不闪烁的稳定光源。")
                    };

                case LunarDay.Day5_Ritual:
                    return new List<NarrativeBeatConfig>
                    {
                        CreateBeat(
                            15,
                            "第二次揭示与决定",
                            "day5_revelation",
                            "先停，再修",
                            "scene_start",
                            "主角意识到真正失控的不是站点，而是自己一遇波动就丢失身体节律，因此决定先停下来。",
                            "如果我现在继续追着它跑，我只会把自己也一起弄丢。",
                            "维护面板亮度主动降低，为仪式让出空间。"),
                        CreateBeat(
                            16,
                            "观众揭示",
                            "day5_audience_reveal",
                            "月球站原本就在测试人的节律",
                            "narration_midpoint",
                            "玩家终于看明白：Quiet Deck 和 Ritual Valve 不是装饰，而是项目组刻意保留的人类节律接口。",
                            "也许这座站从来不只是在测试空气和水。它也在测试，人能不能在这里不靠焦虑活着。",
                            "静区与阀门的设计档被短暂调出。"),
                        CreateBeat(
                            17,
                            "第三次揭示与决定",
                            "day5_third_decision",
                            "重新定义成功",
                            "ritual_start",
                            "主角承认第一周真正的任务不是零事故，而是证明人可以在这里维持清醒。",
                            "如果这地方必须靠我一直紧绷着才能运行，那它还不算真的能住人。",
                            "值守日志标题从 Zero Incident 改成 First Stable Week。"),
                        CreateBeat(
                            18,
                            "过门、磨难、死亡之旅",
                            "day5_gate",
                            "只剩呼吸与阀门",
                            "ritual_deep_phase",
                            "主角进入深度仪式：灯光压低、界面沉默，故事跨过门槛，进入近乎黑场的内在旅程。",
                            "当按钮都沉默下来，我才知道自己有多依赖它们替我回答问题。",
                            "大部分界面熄灭，仅保留阀门冷光与呼吸音。")
                    };

                case LunarDay.Day6_Stability:
                    return new List<NarrativeBeatConfig>
                    {
                        CreateBeat(
                            19,
                            "战斗",
                            "day6_battle",
                            "对抗重新抓回控制权的冲动",
                            "scene_start",
                            "真正的战斗不是修复故障，而是对抗那股想立刻重新抓回全部控制权的冲动。",
                            "我最想做的事，未必是现在最需要做的事。",
                            "系统恢复过程中，仪表逐步稳定但不再被快速拉扯。"),
                        CreateBeat(
                            20,
                            "自我揭示",
                            "day6_self_revelation",
                            "稳定来自节律，不来自绝对控制",
                            "ritual_complete",
                            "主角真正理解：稳定不是外部系统永远无波动，而是自己能带着波动继续存在。",
                            "原来平静不是把自己清空，而是不用再把每个念头都抓住。",
                            "环境音回到低位，动作反馈变得更缓。")
                    };

                case LunarDay.Day7_Reflection:
                    return new List<NarrativeBeatConfig>
                    {
                        CreateBeat(
                            21,
                            "道德决定",
                            "day7_moral_decision",
                            "向地球提交什么样的结论",
                            "scene_start",
                            "主角必须决定：是把第一周写成高效协议的胜利，还是诚实写下‘人类节律必须进入系统’。",
                            "如果我把真正有效的部分删掉，下一任只会更像一台被拧紧的机器。",
                            "值守报告界面停留在结论栏，等待填写。"),
                        CreateBeat(
                            22,
                            "新平衡",
                            "day7_equilibrium",
                            "把修订后的日志留给下一任",
                            "day_completion",
                            "地球升起，主角继续工作，但速度、呼吸和看待任务的方式已经改变，并把修订日志留给下一任。",
                            "我没有学会征服孤独。我只是学会，在孤独出现时先把呼吸放稳。",
                            "地球升起，基地稳定运转，日志被保存。")
                    };

                default:
                    return new List<NarrativeBeatConfig>();
            }
        }

        private static NarrativeBeatConfig CreateBeat(
            int storyStepNumber,
            string storyFunction,
            string beatId,
            string title,
            string trigger,
            string summary,
            string voiceoverLine,
            string environmentCue)
        {
            return new NarrativeBeatConfig
            {
                storyStepNumber = storyStepNumber,
                storyFunction = storyFunction,
                beatId = beatId,
                title = title,
                trigger = trigger,
                summary = summary,
                voiceoverLine = voiceoverLine,
                environmentCue = environmentCue
            };
        }

        private static string GetDayName(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    return "Arrival";
                case LunarDay.Day2_Adaptation:
                    return "Adaptation";
                case LunarDay.Day3_Order:
                    return "Order";
                case LunarDay.Day4_Uncertainty:
                    return "Uncertainty";
                case LunarDay.Day5_Ritual:
                    return "Deep Ritual";
                case LunarDay.Day6_Stability:
                    return "Stability";
                case LunarDay.Day7_Reflection:
                    return "Reflection";
                default:
                    return "Unknown";
            }
        }

        private static string GetTheme(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    return "陌生环境中的安顿";
                case LunarDay.Day2_Adaptation:
                    return "从辨认空间到形成熟悉";
                case LunarDay.Day3_Order:
                    return "用重复建立秩序";
                case LunarDay.Day4_Uncertainty:
                    return "在波动里维持在场";
                case LunarDay.Day5_Ritual:
                    return "把控制欲转为节律";
                case LunarDay.Day6_Stability:
                    return "带着新的注意力返回日常";
                case LunarDay.Day7_Reflection:
                    return "把个人经验接回文明尺度";
                default:
                    return "主题";
            }
        }

        private static string GetEmotionalGoal(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    return "先安顿自己，而不是马上掌控一切。";
                case LunarDay.Day2_Adaptation:
                    return "让陌生空间开始拥有可辨认的节律。";
                case LunarDay.Day3_Order:
                    return "通过重复动作获得低焦虑的秩序感。";
                case LunarDay.Day4_Uncertainty:
                    return "接受波动存在，不用过度控制来掩盖焦虑。";
                case LunarDay.Day5_Ritual:
                    return "把注意力从外部控制拉回身体和呼吸。";
                case LunarDay.Day6_Stability:
                    return "把仪式获得的稳定感带回工作流程。";
                case LunarDay.Day7_Reflection:
                    return "在回望中确认这一周留下的内在变化。";
                default:
                    return "建立稳定感。";
            }
        }

        private static string GetDramaticQuestion(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    return "当所有系统都还不熟悉时，我能先让自己待在这里吗？";
                case LunarDay.Day2_Adaptation:
                    return "熟悉会减少孤独，还是只是让孤独有了形状？";
                case LunarDay.Day3_Order:
                    return "秩序是在保护我，还是在慢慢吞掉我？";
                case LunarDay.Day4_Uncertainty:
                    return "当熟悉的流程失效时，我还能不能留在此刻？";
                case LunarDay.Day5_Ritual:
                    return "如果只剩呼吸和一个阀门，我还需要证明什么？";
                case LunarDay.Day6_Stability:
                    return "平静会让我迟缓，还是让我更清楚？";
                case LunarDay.Day7_Reflection:
                    return "这一周带走的不是结论，而会是什么？";
                default:
                    return "我如何在这里维持清醒？";
            }
        }

        private static string GetExternalPressure(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    return "气闸闭合、系统自检、仪表仍在校准。";
                case LunarDay.Day2_Adaptation:
                    return "维护任务开始重复出现，通信内容减少。";
                case LunarDay.Day3_Order:
                    return "任务清单精确、重复、没有情绪。";
                case LunarDay.Day4_Uncertainty:
                    return "故障噪点、通信延迟和数值波动同时出现。";
                case LunarDay.Day5_Ritual:
                    return "大部分外部控制面板失效，只剩基础生命维持接口。";
                case LunarDay.Day6_Stability:
                    return "系统恢复，但动作惯性仍在，需要重新选择节奏。";
                case LunarDay.Day7_Reflection:
                    return "任务接近尾声，地球重新回到视野中央。";
                default:
                    return "环境保持低刺激但持续施压。";
            }
        }

        private static string GetInnerShift(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    return "从被环境推着走，转为承认自己已经抵达。";
                case LunarDay.Day2_Adaptation:
                    return "从陌生到辨认出属于自己的角落和节奏。";
                case LunarDay.Day3_Order:
                    return "从执行任务到利用秩序安放自己。";
                case LunarDay.Day4_Uncertainty:
                    return "从急于修复转向观察与等待。";
                case LunarDay.Day5_Ritual:
                    return "从向外抓取转向向内归位。";
                case LunarDay.Day6_Stability:
                    return "从被流程拖行转为主动选择动作速度。";
                case LunarDay.Day7_Reflection:
                    return "从个人求稳延展到更长的人类时间感。";
                default:
                    return "从紧绷转向清醒。";
            }
        }

        private static string GetVisualMotif(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    return "冷白舱灯、漂浮灰尘、过亮的月表反光。";
                case LunarDay.Day2_Adaptation:
                    return "固定走廊、被反复擦拭的金属边、规律噪声。";
                case LunarDay.Day3_Order:
                    return "整齐灯带、对齐工具、稳定通风。";
                case LunarDay.Day4_Uncertainty:
                    return "屏幕噪点、闪断灯带、舱壁轻震。";
                case LunarDay.Day5_Ritual:
                    return "黑场、呼吸音、单一阀门的冷光。";
                case LunarDay.Day6_Stability:
                    return "柔和顶灯、安静阴影、缓慢回流的水声。";
                case LunarDay.Day7_Reflection:
                    return "地球升起、舷窗冷凝、基地内外的尺度对照。";
                default:
                    return "低饱和、克制、近乎静止。";
            }
        }

        private static string GetRitualDescription(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    return "用安顿替代征服，让身体比理智先承认抵达。";
                case LunarDay.Day2_Adaptation:
                    return "让熟悉感从空间的角落和声音里慢慢长出来。";
                case LunarDay.Day3_Order:
                    return "把重复动作从任务感转化为低焦虑的节律。";
                case LunarDay.Day4_Uncertainty:
                    return "在故障与波动中练习不立刻冲向控制。";
                case LunarDay.Day5_Ritual:
                    return "在最少控制里重新建立呼吸、动作与注意力的顺序。";
                case LunarDay.Day6_Stability:
                    return "验证平静不是退出工作，而是另一种工作方法。";
                case LunarDay.Day7_Reflection:
                    return "把一周获得的节律从个人身体接回更大的文明视角。";
                default:
                    return "基础仪式。";
            }
        }

        private static string GetRitualIntention(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    return "让身体接受自己已经到达。";
                case LunarDay.Day2_Adaptation:
                    return "让空间从陌生转为可居住。";
                case LunarDay.Day3_Order:
                    return "让重复动作承担稳定功能。";
                case LunarDay.Day4_Uncertainty:
                    return "在波动中练习停留。";
                case LunarDay.Day5_Ritual:
                    return "把控制欲还原成节律与觉察。";
                case LunarDay.Day6_Stability:
                    return "把新的节律带回日常。";
                case LunarDay.Day7_Reflection:
                    return "把个人节律放进更长的时间尺度里。";
                default:
                    return "维持稳定。";
            }
        }

        private static string GetRitualEntryPrompt(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    return "先别解决一切。先让身体知道你已经到了。";
                case LunarDay.Day2_Adaptation:
                    return "找一小块安静的地方，让注意力先落下来。";
                case LunarDay.Day3_Order:
                    return "把下一次重复动作做慢一点，看看它会不会变得不同。";
                case LunarDay.Day4_Uncertainty:
                    return "在修复之前，先确认你还在呼吸。";
                case LunarDay.Day5_Ritual:
                    return "把视线从面板上移开，先确认呼吸还在。";
                case LunarDay.Day6_Stability:
                    return "今天的稳定，不靠更快，而靠更清楚。";
                case LunarDay.Day7_Reflection:
                    return "在看向地球之前，先感觉脚下这一小块地板。";
                default:
                    return "让自己慢下来。";
            }
        }

        private static string GetRitualAnchorLine(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    return "吸气时知道自己在这里，呼气时让肩膀放下。";
                case LunarDay.Day2_Adaptation:
                    return "听一听这间舱室最稳定的那条声音线。";
                case LunarDay.Day3_Order:
                    return "把动作做得更匀，而不是更快。";
                case LunarDay.Day4_Uncertainty:
                    return "让闪烁继续闪烁，你先不用追过去。";
                case LunarDay.Day5_Ritual:
                    return "吸气时感受胸腔抬起，呼气时让肩膀慢下来。";
                case LunarDay.Day6_Stability:
                    return "让今天的第一步和昨天一样稳，但不要一样急。";
                case LunarDay.Day7_Reflection:
                    return "在更大的景象出现前，先感受现在这一次呼吸。";
                default:
                    return "先抓住呼吸。";
            }
        }

        private static string GetRitualObserveLine(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    return "陌生感还在也没关系，它会慢一点地过去。";
                case LunarDay.Day2_Adaptation:
                    return "熟悉不是拥有，而是可以不急着解释。";
                case LunarDay.Day3_Order:
                    return "重复动作里如果有一点安静，就让它多停一秒。";
                case LunarDay.Day4_Uncertainty:
                    return "如果数值还没稳定，你也可以先稳定。";
                case LunarDay.Day5_Ritual:
                    return "如果思绪回来，不要驱赶，只要知道它回来过。";
                case LunarDay.Day6_Stability:
                    return "工作还在继续，但你不必把自己也拧紧。";
                case LunarDay.Day7_Reflection:
                    return "你看到的不只是地球，也是这一周留下的变化。";
                default:
                    return "知道此刻正在发生什么。";
            }
        }

        private static string GetRitualExitPrompt(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    return "你不用适应整个月球，只要适应眼前这一平方米。";
                case LunarDay.Day2_Adaptation:
                    return "现在这间舱室里，已经有一点东西开始属于你。";
                case LunarDay.Day3_Order:
                    return "秩序不必完美，只要足够让你继续前进。";
                case LunarDay.Day4_Uncertainty:
                    return "波动没有结束，但你已经不完全被它带走。";
                case LunarDay.Day5_Ritual:
                    return "当灯重新亮起，你不需要变成另一个人。";
                case LunarDay.Day6_Stability:
                    return "把这个速度带回任务里，不必刻意保留它。";
                case LunarDay.Day7_Reflection:
                    return "把这一周带走的，不是答案，而是更稳的呼吸。";
                default:
                    return "慢慢回到当下。";
            }
        }
    }
}
