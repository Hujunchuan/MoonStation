# MoonStation / Lunar Week One
## Timeline触发表与音频资产命名表

用途：把现有剧情、场景卡、文本库转成可直接给 Timeline、音频实现、字幕表使用的执行规格。

---

## 1. 资产命名总规则

### 1.1 语音旁白 VO

命名格式：

`VO_D{DayNumber}_{Category}_{Index}`

示例：

- `VO_D1_OPEN_01`
- `VO_D3_WORK_02`
- `VO_D7_CLOSE_01`

分类说明：

- `OPEN`：场景起始旁白
- `WORK`：资源管理或中段旁白
- `TURN`：剧情转折旁白
- `CLOSE`：日结尾旁白

### 1.2 前任日志 LOG

命名格式：

`LOG_QY_D{DayNumber}_{Index}`

示例：

- `LOG_QY_D2_01`
- `LOG_QY_D4_02`

### 1.3 地球通信 COM

命名格式：

`COM_GROUND_D{DayNumber}_{Index}`

示例：

- `COM_GROUND_D1_01`
- `COM_GROUND_D7_02`

### 1.4 协议系统 SYS

命名格式：

`SYS_CARETAKER_D{DayNumber}_{Index}`

示例：

- `SYS_CARETAKER_D3_01`
- `SYS_CARETAKER_D5_02`

### 1.5 仪式引导 RIT

命名格式：

`RIT_D{DayNumber}_{Phase}_{Index}`

示例：

- `RIT_D5_ENTER_01`
- `RIT_D5_ANCHOR_01`
- `RIT_D5_EXIT_01`

### 1.6 纪录片回响 ARC

命名格式：

`ARC_{Source}_{Theme}_{Index}`

示例：

- `ARC_APOLLO_CABIN_01`
- `ARC_ISS_CHECKLIST_01`
- `ARC_EARTHRISE_READING_01`

### 1.7 环境音频 CUE

命名格式：

`CUE_{Zone}_{Action}_{Intensity}`

示例：

- `CUE_AIRLOCK_SEAL_LOW`
- `CUE_SYSTEMSRING_FLICKER_MED`
- `CUE_WINDOW_EARTHRISE_SOFT`

---

## 2. 触发类型定义

### 2.1 结构触发

- `scene_start`
- `state_enter_introduction`
- `state_enter_resource`
- `state_enter_narration`
- `state_enter_ritual`
- `state_exit_ritual`
- `day_completion`

### 2.2 交互触发

- `first_resource_interaction`
- `resource_management_midpoint`
- `first_quiet_deck_entry`
- `first_valve_hover`
- `ritual_interaction_count_1`
- `ritual_interaction_count_3`

### 2.3 异常触发

- `anomaly_warning`
- `anomaly_triggered`
- `anomaly_resolved`

### 2.4 观看触发

- `first_window_gaze`
- `terminal_log_opened`
- `report_terminal_opened`

---

## 3. Timeline 组织原则

- 每一天至少有 3 个叙事触发点。
- 同一时段不同时播放两条主旁白。
- `COM` 与 `SYS` 可与环境音叠加，但不应压过 `VO`。
- `ARC` 只作为背景回响使用，不直接抢占叙事主位。
- Day 5 深度仪式中，除 `RIT` 外尽量不播放其他内容。

---

## 4. 7天触发表

### Day 1 - Arrival

| 序号 | 戏剧步骤 | 触发条件 | 推荐状态窗口 | 资产 ID | 字幕/文本来源 | 位置 | 目的 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Step 1 自我揭示 | `scene_start` | Introduction 起点 | `VO_D1_OPEN_01` | `D1_VO_OPEN_01` | Airlock Spine | 建立主角欲望与紧绷状态 |
| 2 | Step 2 鬼魂 | `state_enter_introduction + 10s` | Introduction 中段 | `VO_D1_OPEN_02` | 新增旁白位 | Airlock Spine | 带出旧伤与孤立感 |
| 3 | Step 3 弱点 | `first_resource_interaction` | Resource 开始 | `VO_D1_WORK_01` | `D1_VO_WORK_01` | Systems Ring | 明示主角靠动作压焦虑 |
| 4 | Step 4 激励事件 | `state_enter_resource + 20s` | Resource 前段 | `SYS_CARETAKER_D1_01` | `D1_SYS_01` | Systems Ring | Caretaker Protocol 正式入场 |
| 5 | 世界补充 | `terminal_log_opened` | Resource 中段 | `COM_GROUND_D1_01` | `D1_COM_EARTH_01` | Terminal A | 建立地球远程存在 |
| 6 | 收束 | `day_completion` | Completion | `VO_D1_CLOSE_01` | `D1_VO_CLOSE_01` | Sleeping Niche / Window | 建立“人为定义休息” |

### Day 2 - Adaptation

| 序号 | 戏剧步骤 | 触发条件 | 推荐状态窗口 | 资产 ID | 字幕/文本来源 | 位置 | 目的 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Step 5 欲望 | `scene_start` | Introduction 起点 | `VO_D2_OPEN_01` | `D2_VO_OPEN_01` | Quiet Deck 入口 | 把零事故目标说清楚 |
| 2 | Step 6 盟友 | `first_quiet_deck_entry` | Introduction 中段 | `LOG_QY_D2_01` | `D2_LOG_QY_01` | Quiet Deck Bench | 引入乔榆与空间盟友 |
| 3 | Step 6 盟友强化 | `resource_management_midpoint` | Resource 中段 | `ARC_ISS_HABITATION_01` | 自定义字幕 | Systems Ring 背景 | 强化“被住过的空间”感 |
| 4 | Step 7 谜团 | `first_valve_hover` | Resource 后段 | `VO_D2_WORK_02` | `D2_VO_WORK_02` | Ritual Valve | 引出阀门谜团 |
| 5 | 谜团补充 | `terminal_log_opened` | Narration | `LOG_QY_D2_02` | `D2_LOG_QY_02` | Terminal B | 指出“安静也是信息” |
| 6 | 收束 | `state_enter_ritual` | Ritual 起点 | `VO_D2_CLOSE_01` | `D2_VO_CLOSE_01` | Quiet Deck | 把空间熟悉转成情绪熟悉 |

### Day 3 - Order

| 序号 | 戏剧步骤 | 触发条件 | 推荐状态窗口 | 资产 ID | 字幕/文本来源 | 位置 | 目的 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Step 8 假盟友真对手 | `scene_start` | Introduction 起点 | `VO_D3_OPEN_01` | `D3_VO_OPEN_01` | Systems Ring | 协议把高效伪装成安稳 |
| 2 | Step 8 对手强化 | `state_enter_resource + 15s` | Resource 前段 | `SYS_CARETAKER_D3_01` | `D3_SYS_01` | Systems Ring | 给出效率奖励感 |
| 3 | Step 9 第一次揭示 | `terminal_log_opened` | Resource 中段 | `LOG_QY_D3_02` | `D3_LOG_QY_02` | Terminal C | 四拍记号被读懂 |
| 4 | Step 9 揭示补白 | `resource_management_midpoint` | Resource 中段 | `VO_D3_WORK_02` | `D3_VO_WORK_02` | Systems Ring | 主角意识到前任用节律留住自己 |
| 5 | Step 10 计划 | `state_enter_ritual` | Ritual 起点 | `VO_D3_CLOSE_01` | `D3_VO_CLOSE_01` | Ritual Valve | 决定把节律嵌入维护 |
| 6 | 收束 | `state_enter_narration` | Narration | `ARC_ISS_CHECKLIST_01` | 自定义字幕 | Background | 用纪录片回响衬托秩序感 |

### Day 4 - Uncertainty

| 序号 | 戏剧步骤 | 触发条件 | 推荐状态窗口 | 资产 ID | 字幕/文本来源 | 位置 | 目的 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Step 11 对手计划 | `scene_start` | Introduction 起点 | `SYS_CARETAKER_D4_01` | `D4_SYS_01` | Systems Ring | 对手开始压缩窗口 |
| 2 | Step 11 重击前兆 | `state_enter_resource + 10s` | Resource 前段 | `VO_D4_OPEN_02` | `D4_VO_OPEN_02` | Systems Ring | 点出“效率只会要求更快” |
| 3 | Step 12 Drive | `resource_management_midpoint` | Resource 中段 | `VO_D4_WORK_01` | `D4_VO_WORK_01` | Systems Ring | 主角加倍控制 |
| 4 | Step 13 盟友攻击 | `anomaly_warning` | Resource 后段 | `LOG_QY_D4_01` | `D4_LOG_QY_01` | Terminal D / Overlay | 直接戳破主角问题 |
| 5 | Step 14 表面失败 | `anomaly_triggered` | Transition / Ritual 前 | `VO_D4_WORK_02` | `D4_VO_WORK_02` | Whole Station | 旧办法失效 |
| 6 | 收束 | `state_enter_ritual` | Ritual 起点 | `VO_D4_CLOSE_01` | `D4_VO_CLOSE_01` | Ritual Zone | 接受“波动还在，我也还在” |

### Day 5 - Deep Ritual

| 序号 | 戏剧步骤 | 触发条件 | 推荐状态窗口 | 资产 ID | 字幕/文本来源 | 位置 | 目的 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Step 15 第二次揭示 | `scene_start` | Introduction 起点 | `VO_D5_OPEN_01` | `D5_VO_OPEN_01` | Quiet Deck / Darkened Systems Ring | 先停再修 |
| 2 | Step 16 观众揭示 | `terminal_log_opened` | Narration / Transition | `VO_D5_OPEN_02` | `D5_VO_OPEN_02` | Design Terminal | 月球站原本就在测试人的节律 |
| 3 | Step 17 第三次决定 | `state_enter_ritual` | Ritual 起点 | `VO_D5_WORK_01` | `D5_VO_WORK_01` | Ritual Zone | 重新定义成功 |
| 4 | Step 18 过门 | `ritual_deep_phase` | Ritual Enter | `RIT_D5_ENTER_01` | `D5_RIT_01` | Ritual Zone | 跨过门槛 |
| 5 | Step 18 磨难 | `ritual_interaction_count_1` | Ritual Anchor/Order | `RIT_D5_ANCHOR_01` | `D5_RIT_02` | Ritual Valve | 让身体重新回到呼吸 |
| 6 | Step 18 深潜 | `ritual_interaction_count_3` | Ritual Order/Observe | `VO_D5_WORK_02` | `D5_VO_WORK_02` | Ritual Valve | 把阀门动作重新解释为节律 |
| 7 | Step 18 出仪 | `ritual_complete` | Ritual Exit | `RIT_D5_EXIT_01` | `D5_RIT_05` | Ritual Zone | 完成黑场回归 |
| 8 | 收束 | `state_exit_ritual` | Completion | `VO_D5_CLOSE_01` | `D5_VO_CLOSE_01` | Quiet Deck | 点出真正得到的不是“胜利感” |

### Day 6 - Stability

| 序号 | 戏剧步骤 | 触发条件 | 推荐状态窗口 | 资产 ID | 字幕/文本来源 | 位置 | 目的 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Step 19 战斗 | `scene_start` | Introduction 起点 | `VO_D6_OPEN_02` | `D6_VO_OPEN_02` | Systems Ring | 战斗变成对抗旧习惯 |
| 2 | Step 19 战斗深化 | `state_enter_resource + 15s` | Resource 前段 | `LOG_QY_D6_02` | `D6_LOG_QY_02` | Terminal E | “慢但稳不是退步” |
| 3 | Step 20 自我揭示 | `resource_management_midpoint` | Resource 中段 | `VO_D6_WORK_01` | `D6_VO_WORK_01` | Systems Ring | 明白什么时候该动什么时候不必动 |
| 4 | Step 20 自我揭示强化 | `state_enter_ritual` | Ritual 起点 | `VO_D6_WORK_02` | `D6_VO_WORK_02` | Quiet Deck | 平静不是退后 |
| 5 | 收束 | `day_completion` | Completion | `VO_D6_CLOSE_01` | `D6_VO_CLOSE_01` | Systems Ring Exit | 把“平静不是停工”说清楚 |

### Day 7 - Reflection

| 序号 | 戏剧步骤 | 触发条件 | 推荐状态窗口 | 资产 ID | 字幕/文本来源 | 位置 | 目的 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Step 21 道德决定 | `scene_start` | Introduction 起点 | `COM_GROUND_D7_02` | `D7_COM_EARTH_02` | Report Terminal | 地球在等待结论 |
| 2 | Step 21 决定深化 | `report_terminal_opened` | Resource/Narration | `VO_D7_OPEN_02` | `D7_VO_OPEN_02` | Report Terminal | 主角决定不删去“人的节律”这件事 |
| 3 | Step 22 新平衡 | `first_window_gaze` | Ritual/Completion | `VO_D7_WORK_01` | `D7_VO_WORK_01` | Window Gallery | Earthrise 作为尺度装置 |
| 4 | Step 22 新平衡强化 | `state_enter_completion` | Completion | `VO_D7_WORK_02` | `D7_VO_WORK_02` | Window Gallery | 把个人经验抬到文明层 |
| 5 | Step 22 结尾 | `day_completion` | Final End | `VO_D7_CLOSE_01` | `D7_VO_CLOSE_01` | Report Terminal / Window | 留给下一任的最终句子 |
| 6 | 尾声文档 | `day_completion + 3s` | Final End | `LOG_QY_D7_02` | `D7_LOG_QY_02` | Saved Log Screen | 日志被正式传承下去 |

---

## 5. 字幕表建议字段

字幕 CSV / Sheet 建议字段：

- `subtitle_id`
- `asset_id`
- `speaker`
- `language`
- `text`
- `day`
- `scene`
- `trigger_type`
- `location`
- `notes`

---

## 6. 音频混音建议

- `VO` 永远优先级最高。
- `SYS` 比 `COM` 更干、更窄、更近。
- `ARC` 要像隔着一道墙或隔着时间传来。
- Day 5 深度仪式时，除了阀门反馈和呼吸底噪，其他层级都应明显后退。
- Earthrise 不用大音乐，不要把结尾做成“胜利镜头”。
