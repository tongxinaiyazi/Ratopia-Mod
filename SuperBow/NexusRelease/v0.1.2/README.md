# Super Bow 0.1.2 — Nexus Release Kit

本目录是可直接用于 Nexus Mods 的发布资源，不应整体打包上传。

## 使用顺序

1. 使用 `NEXUS_TITLE.txt`、`NEXUS_SUMMARY.txt` 和 `NEXUS_DESCRIPTION.txt` 填写 Mod 页面。
2. 上传 `images/SuperBow-Cover-1280x720.png` 作为封面。
3. 上传 `files/SuperBow-v0.1.2-BepInEx5.zip` 作为唯一 Main File。
4. 使用 `FILE_DESCRIPTION.txt` 和 `CHANGELOG.txt` 填写 Files 页面。
5. 按 `CREDITS_AND_PERMISSIONS.md` 与 `UPLOAD_CHECKLIST.md` 配置 Credits、权限和披露。
6. 上传前从项目根目录运行：

```powershell
& .\scripts\Test-NexusRelease.ps1
```

## 重要边界

- Mod 下载 ZIP 的安装位置是 `BepInEx/plugins/SuperBow/SuperBow.dll`。
- 原版弓箭图标只作为 Nexus 页面图像，不在 Mod ZIP 内发布。
- `metadata.json` 是字段备忘录，不需要上传。
- 本资源对应且仅对应 `0.1.2`。
