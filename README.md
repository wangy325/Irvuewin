# Irvue-win
An [Irvue](https://apps.apple.com/us/app/irvue/id1039633667?mt=12) like windows wallpaper tool. Using [Unsplash](https://unsplash.com) API.

## TODOs

- [ ] 删除所选中的频道时，画廊的内容没有更新
- [ ] 计时器在重启/重新唤醒后会重置
- [ ] 按照分辨率过滤壁纸
- [ ] 优化photo show case 当photo为空时的显示
- [ ] 优化应用启动速度
- [ ] Refactor: Use SQLite
- [x] 某些频道不存在特性方向的壁纸，这时候会出现问题
- [x] 设置界面切换壁纸方向的时候，壁纸和channel对不上
- [x] 搞清楚load more photos的触发机制。为什么打开窗口的时候会触发
- [x] 并不是所有的加载壁纸都去要放到VM属性里面去
- [x] 壁纸管理窗口打开是 没有默认选中的元素
- [x] 刷新壁纸队列的逻辑有bug
- [x] 多显示器下壁纸详情显示有bug
- [x] 消息框
- [x] Feature: 清除缓存 重置应用
- [x] 计时器睡眠时停机
- [x] 当使用不同分辨率的显示器时，托盘应用的弹出位置不正常
- [x] i18n
- [x] 解耦频道选择的listbox和radiobutton
- [x] Feature: multi display support
- [x] 频道选择的radio button闪退
- [x] 添加频道和频道管理弹出2个窗口
- [x] previous wallpaper后没有更新壁纸信息
- [x] 总图片数从磁盘缓存获取	
- [x] 重刷壁纸后，sequence是否要重置？ Yes
- [x] 设置里更改壁纸偏好后，应该重刷壁纸
- [x] 频道图片预览刷新后，不能连续加载了
- [x] channel条目过多后tray不显示
- [x] 检查所有的双向绑定
- [x] Feature: 随windows启动
- [x] 自动更新壁纸后需要更新nextTrigger
- [x] 改channelsviewmodel为单例
- [x] Feature: 定时调度逻辑
- [x] channel管理窗listBox selectitem和radio checkItem不绑定
- [x] 当打开添加页面时，显示提示信息
