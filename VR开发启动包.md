# Lunar Week One VR - 7天开发启动包

> 版本: 1.0
> 更新: 2026-03-12

---

# 第一部分：Unity VR 项目结构

## 1.1 项目初始化

### 环境要求

| 软件 | 版本 | 说明 |
|------|------|------|
| Unity | 2022.3 LTS | 稳定，推荐 |
| Meta Quest SDK | 65.0+ | Quest 3 支持 |
| XR Interaction Toolkit | 2.5.0 | VR 交互框架 |

### 安装步骤

```bash
# 1. Unity Hub 下载 2022.3 LTS
# 2. 创建新项目 → 3D (URP)
# 3. Package Manager 安装:
#    - XR Interaction Toolkit
#    - XR Plugin Management
#    - Meta XR SDK (从 https://developer.oculus.com 下载)
```

## 1.2 目录结构

```
LunarWeekOne/
├── Assets/
│   ├── _Project/
│   │   ├── Scripts/
│   │   │   ├── Core/
│   │   │   │   ├── GameManager.cs
│   │   │   │   ├── SceneManager.cs
│   │   │   │   └── DataManager.cs
│   │   │   ├── VR/
│   │   │   │   ├── VRRigSetup.cs
│   │   │   │   ├── HandTracking.cs
│   │   │   │   ├── LocomotionController.cs
│   │   │   │   └── VRInputManager.cs
│   │   │   ├── Ritual/
│   │   │   │   ├── RitualEngine.cs
│   │   │   │   ├── RitualPhase.cs
│   │   │   │   ├── BreathingGuide.cs
│   │   │   │   └── AudioScheduler.cs
│   │   │   ├── Audio/
│   │   │   │   ├── SpatialAudioManager.cs
│   │   │   │   ├── AmbientSoundscape.cs
│   │   │   │   └── BiSyncAudio.cs
│   │   │   ├── Narrative/
│   │   │   │   ├── NarrativeTrigger.cs
│   │   │   │   ├── MonologuePlayer.cs
│   │   │   │   └── VideoSequence.cs
│   │   │   ├── UI/
│   │   │   │   ├── VRCanvas.cs
│   │   │   │   ├── WorldSpaceButton.cs
│   │   │   │   └── ProgressDisplay.cs
│   │   │   └── Systems/
│   │   │       ├── DayProgress.cs
│   │   │       ├── ResourceSystem.cs
│   │   │       ├── BuildingSystem.cs
│   │   │       └── SaveSystem.cs
│   │   │
│   │   ├── Prefabs/
│   │   │   ├── VR/
│   │   │   │   ├── XR Origin.prefab
│   │   │   │   ├── LeftHand.prefab
│   │   │   │   └── RightHand.prefab
│   │   │   ├── Ritual/
│   │   │   │   ├── BreathSphere.prefab
│   │   │   │   ├── FocusPoint.prefab
│   │   │   │   └── RitualPlatform.prefab
│   │   │   └── UI/
│   │   │       ├── VRCanvas.prefab
│   │   │       └── WorldSpacePanel.prefab
│   │   │
│   │   ├── Scenes/
│   │   │   ├── 0_Bootstrap.unity
│   │   │   ├── 1_MainMenu.unity
│   │   │   ├── 2_LunarBase.unity
│   │   │   ├── 3_RitualSpace.unity
│   │   │   └── 4_EndSequence.unity
│   │   │
│   │   ├── Materials/
│   │   │   ├── Environment/
│   │   │   ├── Ritual/
│   │   │   └── UI/
│   │   │
│   │   ├── Shaders/
│   │   │   ├── Unlit_TransparentFade.shader
│   │   │   ├── Glow_Pulsing.shader
│   │   │   └── Hologram.shader
│   │   │
│   │   └── Animations/
│   │       ├── Hands/
│   │       └── UI/
│   │
│   ├── ThirdParty/
│   │   ├── XR Interaction Toolkit/
│   │   ├── Meta XR SDK/
│   │   └── FMOD/
│   │
│   ├── Audio/
│   │   ├── Music/
│   │   ├── Ambience/
│   │   ├── Rituals/
│   │   └── SFX/
│   │
│   └── Resources/
│       ├── Config/
│       │   ├── DayConfig.json
│       │   ├── RitualConfig.json
│       │   └── Settings.json
│       └── Data/
│           └── SaveData.json
│
├── Packages/
│
├── ProjectSettings/
│
└── README.md
```

## 1.3 核心脚本模板

### VR Rig 设置

```csharp
// Assets/_Project/Scripts/VR/VRRigSetup.cs
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class VRRigSetup : MonoBehaviour
{
    [Header("XR Origin")]
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Camera xrCamera;

    [Header("Controllers")]
    [SerializeField] private XRController leftController;
    [SerializeField] private XRController rightController;

    [Header("Locomotion")]
    [SerializeField] private TeleportationProvider teleportProvider;
    [SerializeField] private ContinuousMoveProvider moveProvider;

    private void Start()
    {
        // 初始化 VR 设备
        InitializeVR();
    }

    private void InitializeVR()
    {
        // 检查 XR 支持
        if (XRSettings.isDeviceActive)
        {
            Debug.Log($"VR Active: {XRSettings.loadedDeviceName}");
            // 设置推荐刷新率
            Application.targetFrameRate = 72;
        }
        else
        {
            Debug.LogWarning("No VR device detected - running in desktop mode");
        }
    }

    public void SetTeleportEnabled(bool enabled)
    {
        if (teleportProvider)
            teleportProvider.enabled = enabled;
    }
}
```

### 仪式引擎核心

```csharp
// Assets/_Project/Scripts/Ritual/RitualEngine.cs
using System.Collections;
using UnityEngine;

public enum RitualPhase
{
    None,
    Enter,      // 入仪 - 准备
    Anchor,    // 安身 - 呼吸定心
    Order,     // 定序 - 简单重复动作
    Observe,   // 观照 - 内在觉察
    Exit       // 出仪 - 回归
}

public class RitualEngine : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private RitualPhase currentPhase;
    [SerializeField] private float phaseDuration = 60f;

    [Header("References")]
    [SerializeField] private AudioSource ambientAudio;
    [SerializeField] private AudioSource voiceGuideAudio;
    [SerializeField] private GameObject visualElements;
    [SerializeField] private BreathingGuide breathingGuide;

    [Header("Events")]
    public System.Action<RitualPhase> OnPhaseChanged;
    public System.Action OnRitualComplete;

    private bool isRunning = false;

    public void StartRitual()
    {
        if (isRunning) return;
        StartCoroutine(RitualSequence());
    }

    public void StopRitual()
    {
        StopAllCoroutines();
        isRunning = false;
        TransitionToPhase(RitualPhase.None);
    }

    private IEnumerator RitualSequence()
    {
        isRunning = true;

        // 入仪
        yield return StartCoroutine(RunPhase(RitualPhase.Enter, 30f));

        // 安身
        yield return StartCoroutine(RunPhase(RitualPhase.Anchor, 90f));

        // 定序
        yield return StartCoroutine(RunPhase(RitualPhase.Order, 60f));

        // 观照
        yield return StartCoroutine(RunPhase(RitualPhase.Observe, 120f));

        // 出仪
        yield return StartCoroutine(RunPhase(RitualPhase.Exit, 30f));

        isRunning = false;
        OnRitualComplete?.Invoke();
    }

    private IEnumerator RunPhase(RitualPhase phase, float duration)
    {
        TransitionToPhase(phase);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            OnPhaseUpdate(phase, elapsed / duration);
            yield return null;
        }
    }

    private void TransitionToPhase(RitualPhase phase)
    {
        currentPhase = phase;
        OnPhaseChanged?.Invoke(phase);

        // 根据阶段调整音频
        switch (phase)
        {
            case RitualPhase.Enter:
                // 柔和进入
                break;
            case RitualPhase.Anchor:
                breathingGuide.StartBreathing(6f); // 6 次呼吸/分钟
                break;
            case RitualPhase.Observe:
                // 最安静的状态
                break;
        }
    }

    private void OnPhaseUpdate(RitualPhase phase, float progress)
    {
        // 更新视觉效果 - 进度相关
    }
}
```

### 呼吸引导

```csharp
// Assets/_Project/Scripts/Ritual/BreathingGuide.cs
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BreathingGuide : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Transform breathSphere;
    [SerializeField] private Renderer sphereRenderer;
    [SerializeField] private Light ambientLight;

    [Header("Settings")]
    [SerializeField] private float inhaleDuration = 4f;
    [SerializeField] private float exhaleDuration = 4f;
    [SerializeField] private float breathScaleMin = 0.5f;
    [SerializeField] private float breathScaleMax = 1.0f;

    private bool isActive = false;
    private float currentBreathTime = 0f;
    private float targetBreathsPerMinute = 6f;

    private Material sphereMat;

    private void Start()
    {
        sphereMat = sphereRenderer.material;
    }

    public void StartBreathing(float breathsPerMinute)
    {
        targetBreathsPerMinute = breathsPerMinute;
        isActive = true;

        float cycleDuration = 60f / breathsPerMinute;
        inhaleDuration = cycleDuration * 0.5f;
        exhaleDuration = cycleDuration * 0.5f;
    }

    public void StopBreathing()
    {
        isActive = false;
    }

    private void Update()
    {
        if (!isActive) return;

        currentBreathTime += Time.deltaTime;
        float cycleDuration = inhaleDuration + exhaleDuration;
        float phase = (currentBreathTime % cycleDuration) / cycleDuration;

        if (phase < inhaleDuration / cycleDuration)
        {
            // 吸气
            float t = phase / (inhaleDuration / cycleDuration);
            float scale = Mathf.Lerp(breathScaleMin, breathScaleMax, t);
            breathSphere.localScale = Vector3.one * scale;

            // 颜色：吸气 - 偏冷
            sphereMat.SetColor("_EmissionColor", Color.Lerp(Color.blue * 0.3f, Color.cyan * 0.8f, t));
            ambientLight.intensity = Mathf.Lerp(0.3f, 0.8f, t);
        }
        else
        {
            // 呼气
            float t = (phase - inhaleDuration / cycleDuration) / (exhaleDuration / cycleDuration);
            float scale = Mathf.Lerp(breathScaleMax, breathScaleMin, t);
            breathSphere.localScale = Vector3.one * scale;

            // 颜色：呼气 - 偏暖
            sphereMat.SetColor("_EmissionColor", Mathf.Lerp(0.8f, 0.3f, t) * Color.cyan);
            ambientLight.intensity = Mathf.Lerp(0.8f, 0.3f, t);
        }
    }
}
```

---

# 第二部分：XR 技能学习清单

## 2.1 必学技能

### 基础级（1-2天）

| 技能 | 学习资源 | 掌握标准 |
|------|---------|---------|
| Unity 基础操作 | Unity 官方教程 | 能创建场景、导入资源 |
| C# 基础 | 《C# 入门》 | 理解类、接口、协程 |
| XR Interaction Toolkit | [Unity XRIT 文档](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.5) | 能配置手柄交互 |
| Meta Quest 开发 | [Oculus 开发者文档](https://developer.oculus.com/resources) | 能打包 APK |

### 进阶级（3-4天）

| 技能 | 学习资源 | 掌握标准 |
|------|---------|---------|
| VR 移动方案 | XRIT Locomotion | 实现瞬移 + 连续移动 |
| 手势交互 | Unity Hand Tracking | 实现抓取、释放 |
| 3D UI 设计 | World Space Canvas | 创建 VR 友好 UI |
| URP 渲染 | URP 文档 | 调整材质、光照 |

### 高级级（5-7天）

| 技能 | 学习资源 | 掌握标准 |
|------|---------|---------|
| 空间音频 | FMOD / Unity Audio | 3D 音效定位 |
| 性能优化 | Unity Profiler | 稳定 72FPS |
| 打包发布 | App Lab 文档 | 成功上传 |

## 2.2 推荐学习路径

```
Day 1: Unity 基础 → 创建项目 → XR Plugin 安装
    ↓
Day 2: XRIT 基础 → 配置 XR Origin → 手柄测试
    ↓
Day 3: 交互系统 → 抓取/放置 → UI 交互
    ↓
Day 4: 移动系统 → 瞬移 → 连续移动
    ↓
Day 5: 音频系统 → 3D 音效 → 背景氛围
    ↓
Day 6: 场景搭建 → 仪式空间 → 优化
    ↓
Day 7: 打包 → 测试 → 发布
```

## 2.3 关键文档链接

| 主题 | 链接 |
|------|------|
| XR Interaction Toolkit | https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.5 |
| Meta XR SDK | https://developer.oculus.com/resources/ |
| Unity URP | https://docs.unity3d.com/Manual/urp-get-started.html |
| App Lab 发布 | https://developer.oculus.com/distribute/ |

---

# 第三部分：免费资源合集

## 3.1 3D 资产

### 月球/太空主题

| 资源名 | 来源 | 许可证 |
|--------|------|--------|
| Kenney Space Kit | [Itch.io](https://kenney.itch.io/kenney-space-kit) | CC0 |
| Free Space Backgrounds | [Itch.io](https://itch.io/game-assets/tag-space) | 各异 |
| NASA 3D Resources | [NASA](https://nasa3d.arc.nasa.gov/) | 公共域 |

### VR 优化资产

| 资源名 | 来源 | 适用 |
|--------|------|------|
| Low Poly Nature | [Itch.io](https://itch.io/game-assets/tag-low-poly) | 环境 |
| VR Template | [Unity Asset Store](https://assetstore.unity.com/packages/templates/xr-vr-template-201910) | 基础 |

## 3.2 音频资源

### 背景氛围

| 资源名 | 来源 | 许可证 |
|--------|------|--------|
| Freesound | [freesound.org](https://freesound.org/) | CC0/各异 |
| Incompetech | [incompetech.com](https://incompetech.com/) | CC0 |
| Free Music Archive | [freemusicarchive.org](https://www.freemusicarchive.org/) | CC |

### 音效

| 类型 | 推荐搜索 (Freesound) |
|------|---------------------|
| 太空环境 | "space ambience", "spaceship interior" |
| UI 反馈 | "ui click", "soft ping" |
| 仪式 | "chime", "bowl", "meditation" |

## 3.3 工具/插件

### 免费（直接安装）

| 工具 | 来源 | 用途 |
|------|------|------|
| XR Interaction Toolkit | Unity Package Manager | VR 交互 |
| XR Plugin Management | Unity Package Manager | XR 平台支持 |
| TextMeshPro | Unity Package Manager | 文本渲染 |
| Shader Graph | Unity Package Manager | 自定义 Shader |

### 免费（需下载）

| 工具 | 来源 | 用途 |
|------|------|------|
| Meta XR SDK | [Oculus 开发者](https://developer.oculus.com/downloads/package/unity-integration/) | Quest 支持 |
| Meta XR Interaction Framework | [GitHub](https://github.com/oculus-samples/Unity-XR-Interaction-Framework) | 手势交互 |

## 3.4 学习资源

### 视频教程

| 教程 | 作者 | 链接 |
|------|------|------|
| VR 开发入门 | Bracer | [YouTube](https://youtube.com/) 搜索 |
| XRIT 2.0 教程 | Unity | [官方视频](https://learn.unity.com/) |

### 文档

| 主题 | 链接 |
|------|------|
| XR Interaction Toolkit | https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest |
| Quest 开发快速开始 | https://developer.oculus.com/documentation/unity/unity-quick-start/ |

---

# 第四部分：7天详细计划

## Day 1：环境搭建

### 目标
- Unity 项目创建
- VR 开发环境配置
- Quest 设备连接

### 任务清单

```markdown
□ 安装 Unity 2022.3 LTS
□ 创建 3D (URP) 项目
□ 安装 XR Interaction Toolkit (2.5+)
□ 安装 XR Plugin Management
□ 下载并导入 Meta XR SDK
□ 配置 Player Settings → Android → VR
□ 创建测试场景，运行验证
```

### 验收标准
- [ ] Quest 链接成功
- [ ] VR 预览模式正常
- [ ] 手柄显示正常

---

## Day 2：VR 基础交互

### 目标
- XR Origin 配置
- 手柄抓取/放置
- 瞬移移动

### 任务清单

```markdown
□ 配置 XR Origin (XR Rig)
□ 添加 XR Controller (Left/Right)
□ 配置 Ray Interactor
□ 实现物体抓取 (XR Grab Interactable)
□ 实现瞬移 (Teleportation Provider)
□ 创建 VR UI (World Space Canvas)
□ 测试所有交互
```

### 验收标准
- [ ] 能抓取并扔物体
- [ ] 能通过瞬移移动
- [ ] UI 按钮可点击

---

## Day 3：场景与视觉

### 目标
- 月球基地场景
- 视觉风格化
- 光照设置

### 任务清单

```markdown
□ 导入免费月球资产
□ 创建月球基地室内场景
□ 配置 URP 渲染
□ 添加光照 (Baked GI)
□ 创建仪式空间场景
□ 实现渐变过渡效果
□ 添加简单粒子效果
```

### 验收标准
- [ ] 场景运行 72FPS
- [ ] 视觉效果符合"克制、沉浸"调性

---

## Day 4：音频系统

### 目标
- 空间音频
- 背景氛围
- 仪式音频

### 任务清单

```markdown
□ 配置 Audio Source (3D 设置)
□ 导入太空环境音效
□ 创建 AudioMixer
□ 实现声音淡入淡出
□ 创建仪式专用音轨
□ 实现呼吸同步音频
□ 添加语音引导 AudioSource
```

### 验收标准
- [ ] 音效有空间感
- [ ] 仪式音频自动播放

---

## Day 5：核心功能

### 目标
- 仪式引擎
- 进度系统
- 7天体验框架

### 任务清单

```markdown
□ 实现 RitualEngine.cs
□ 实现 BreathingGuide.cs
□ 实现 DayProgress.cs
□ 创建 7 天配置数据
□ 实现进度保存 (PlayerPrefs)
□ 实现主菜单
□ 连接所有系统
```

### 验收标准
- [ ] 能完成一个完整仪式流程
- [ ] 进度正确保存

---

## Day 6：内容填充

### 目标
- 1-2 天完整内容
- UI 完善
- 优化

### 任务清单

```markdown
□ 完善 Day 1 内容 (开头场景)
□ 完善 Day 2 内容 (资源管理)
□ 添加语音引导文本
□ 添加结束画面
□ 性能优化 (Profiling)
□ 修复已知 Bug
□ 内部测试
```

### 验收标准
- [ ] 能完成 1-2 天体验
- [ ] 帧率稳定 72FPS

---

## Day 7：发布

### 目标
- 打包 APK
- 发布到 Quest
- 宣传

### 任务清单

```markdown
□ 打包 Android APK
□ 开启 Quest 开发者模式
□ 安装到 Quest 测试
□ 创建商店截图/视频
□ 发布到 App Lab / Itch.io
□ 编写发布说明
□ 社交媒体宣传
```

### 验收标准
- [ ] APK 成功安装
- [ ] Quest 能正常运行
- [ ] 已发布上线

---

# 附录

## A. 常见问题

### Q: Quest 开发者模式怎么开启？
A: 手机下载 Meta Quest App → 设备 → 开发者模式 → 开启

### Q: 帧率不够怎么办？
A:
- 减少 Draw Calls (批处理)
- 降低分辨率 (XR Settings → Render Scale 0.8)
- 关闭阴影 / 简化 Shader

### Q: 手势不灵敏？
A: 调整 XR Controller → Tracking → Position/Rotation 缩放

---

## B. 技术支持

| 问题 | 解决 |
|------|------|
| XR Plugin 不显示 | Window → XR → Plugin Management |
| Quest 不识别 | 检查 USB 驱动 / 重新安装 Meta Quest App |
| 打包失败 | Build Settings → Switch Platform → Android |

---

*文档版本 1.0 | Lunar Week One VR*
