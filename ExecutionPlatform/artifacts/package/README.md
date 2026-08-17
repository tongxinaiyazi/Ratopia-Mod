# 处刑台

`处刑台` 是一个用于 Ratopia 1.0.0600 Mono 版的 BepInEx 5 Mod。它复用原版监狱的图标、蓝图、世界模型和动画，不包含任何游戏资源或自定义贴图。

## 兼容环境

- Ratopia 1.0.0600 Mono
- BepInEx 5.4.23.5
- Harmony 2.9.0.0
- 插件 GUID：`cn.ratopia.executionplatform`
- Mod 版本：`0.1.1`

其他游戏版本未经过合同测试。插件会核对关键游戏接口；发布脚本还会核对 `Assembly-CSharp.dll` 的 SHA-256。

## 功能

- 开局解锁“处刑台”建筑，建造尺寸、材料、耐久和建造时间与原版监狱相同。
- 详情页和菜单图标复用监狱图片，菜单卡片使用普通建筑背景。
- 使用建筑的标准岗位界面指定一名普通鼠民。
- 被指定的鼠民立即中断普通日程并前往处刑台，该任务以最高调度优先级抢占普通行为。
- 到达可工作位置后播放监狱动作；经过一秒受暂停和游戏速度影响的游戏时间后，生命值归零并进入原版死亡流程。
- 建筑停用或拆除、鼠民换岗、受伤、被监禁、死亡以及切换存档都会无伤害取消当前倒计时。
- 倒计时不写入存档。读档后，如果岗位仍有效，鼠民会重新前往并从头计时。

女王、鼠鼠机器人、儿童、已受伤、被监禁、已死亡或远征中的单位不会成为有效处刑目标。

## 安装

1. 必须先让 Ratopia 游戏完全退出，不要在游戏运行时覆盖插件 DLL。
2. 备份准备验收的专用测试存档；如果已经安装旧版插件，也要备份旧 DLL。
3. 将发布包解压到 Ratopia 游戏目录。最终 DLL 路径应为：

   `BepInEx\plugins\ExecutionPlatform\ExecutionPlatform.dll`

4. 启动游戏，在 BepInEx 日志中确认处刑台的十组 Harmony 补丁和建筑数据库注册均成功。

从源码安装时可使用 `scripts\Install.ps1`。该脚本要求显式指定专用测试存档，检测 Ratopia 已退出，备份该存档和旧插件，并在复制后核对 SHA-256；项目构建本身不会直接写入游戏目录。

不要使用正式存档进行处刑、拆除或卸载验收。首次验证必须使用专用测试存档。

## 验收建议

在专用测试存档中依次检查：建筑菜单、材料与尺寸、建造、岗位选择、立即前往、暂停和不同游戏速度、一秒边界、原版死亡后果、中途换岗或停用取消、连续处刑、保存读档以及拆除。

## 卸载

1. 保持 Mod 已安装，载入需要保留的存档。
2. 拆除所有处刑台并另存到新存档。
3. 正常退出游戏并备份该存档。
4. 删除 `BepInEx\plugins\ExecutionPlatform\ExecutionPlatform.dll`。
5. 重新启动原版游戏，确认新存档可以正常读取。

存档中的处刑台使用自定义建筑值 `10001`。未先拆除建筑就移除 Mod，可能导致原版无法正确恢复这些建筑，因此不要直接在含有处刑台的存档上卸载。

## 构建与打包

```powershell
dotnet test .\ExecutionPlatform.sln -c Release -v minimal "/p:RatopiaDir=E:\steam\steamapps\common\Ratopia" "/p:InstallAfterBuild=false"
.\scripts\Package.ps1 -RatopiaDir 'E:\steam\steamapps\common\Ratopia'
```

发布包只包含 `ExecutionPlatform.dll` 和本 README，不包含 `Assembly-CSharp.dll`、Unity、BepInEx、Harmony、PDB 或任何游戏资源。
