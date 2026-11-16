# 2026-11-17：1.0.33.4 私有商人检测到但仍显示原版

## 证据
- 用户截图确认 neowmerchant 已选中但仍为蓝衣原版商人。
- godot.log 2810 等位置已记录成功重放 neowmerchant 的一个外观初始化步骤，作者无应用失败告警，因此并非漏执行回调。
- 真实 DLL 的 MerchantRuntime.FindTargets 迭代器通过 Spine GetData 的 Resource.ResourcePath 严格比较原版身体/手部骨架路径。切换器把被替换的资源保留在私有 alias/retained 路径，导致目标列表为空，作者异步重试 30 帧后结束。
- 同段日志 NStandardMerchantRoom._Ready 的 NullReferenceException 出现在图鉴 AddChild。只跳过基类 Ready 仍会执行派生类商品/地图/局内逻辑，属于独立预览生命周期遗漏。

## 修复
- 新 PrivateMerchantResourceIdentity：只在已确认的私有商人提供者内，适配实际含原版商人骨架字符串和 ResourcePath getter 的匹配方法，包括编译生成的异步迭代器；不修改第三方磁盘 DLL/PCK。
- 资源身份通过弱引用对象表记录；IgnoreDeep 复制出的依赖仅接受切换器专用路径内的两个完整原版骨架后缀。外部路径、作者私有替换骨架不做猜测。
- 只改变作者目标比较的读取结果，不改 ResourcePath、缓存所有权和现有原生资源隔离；安装按程序集去重。
- 新版 Standard/Blitz 具体房间 Ready 同样跳过预览游戏逻辑，在预览分支重放所选商人；真实商店保留原生 Ready 和库存状态。

## 验证
- 实际 neowmerchant DLL/PCK：真实匹配迭代器安装一次，执行身体/手部 getter+比较契约，隔离路径负对照不匹配而生产适配后匹配；原 ResourcePath 未改变，私有模型及任意外部后缀保持原值，IgnoreDeep 复制同样识别。
- 资源绑定测试通过实际 BindRuntimeDependency 模拟商人骨架缓存置换，验证别名保留与作者身份兼容；原有 200 次资源置换/远端恢复断言保留。
- 两种新版具体商店真实 Ready 补丁发现及预览/局内门控断言通过；商人生命周期、十字军/贞德完整保护、瓦库默认/彩蛋等回归通过。
- Release 构建 0 警告 0 错误；未控制游戏做实机渲染验证，离线不覆盖第三方视口实际绘制。

版本 1.0.33.4，公开版本仍 1.0.33；三文件同步四处，提交推送 Git，不上传创意工坊。
