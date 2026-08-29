# 每日进度 v1.1.0 发布检查

状态：**ready with known limitations**

## 已通过

- Windows 构建成功，文件版本 `1.1.0.0`，产品版本 `1.1.0`。
- 最终 ZIP 可正常解压，共 6 个文件，仅包含 1 个预期 EXE。
- 未发现 `.cs`、`.pdb`、`.sln`、日志或脚本等源代码／调试载荷。
- ZIP SHA-256 校验通过：`5311E706FD5A50EBA8C65F01A6A9A1696428E02558B2FE1C4D7C8FF8864BEC0D`。
- 完成型任务手动排序、首尾边界与新任务末尾追加测试通过。
- 窗口“放大 → 缩小”后的纵向滚动回归测试通过。
- 使用隔离的演示数据完成界面截图核对，未包含用户任务数据。

## 发布包

- `每日进度-v1.1.0-Windows-便携版.zip`
- `每日进度-v1.1.0-Windows-便携版.zip.sha256.txt`

## 已知限制

- 当前为便携版，不包含 MSI/MSIX 安装向导。
- 程序未进行商业代码签名，从互联网下载后可能触发 Windows SmartScreen 提示。
- 依赖 Windows 10/11 中的 .NET Framework 4.x，不是完全自包含运行时。
- 未在全新 Windows 虚拟机上完成跨机器首次启动与 SmartScreen 实测。

## 数据与升级

任务数据保存在 `%LOCALAPPDATA%\DailyProgressDesk`，不在 ZIP 内。升级时应先退出旧程序，再替换 EXE；建议提前备份 `tasks.json`。
