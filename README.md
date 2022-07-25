# Lingguo(灵果)

Multilingual support for Unity game

Unity多语言支持

## 特性

- 零代码，编辑器中就可以完成静态资源的绑定和自动切换
- 所见即所得，编辑器中实时预览不同语言版本切换
- 语言模板，解决多语言翻译语素顺序不一致的文本替换问题
- C#特性支持，使用特性[Language("key")]就可以绑定C#脚本中的变量，实现自动切换语言
- 语言资源基于Addressable，可以运行时下载及更新。也可以和项目自有的ab管理系统并存，互不影响

## 准备工作

1. #### 安装com.blg.gtc.lingguo包
   1. 打开Unity包管理器,点击+按钮,选择Add package from git url,输入https://github.com/treen/lingguo.git ,点击Add按钮，等待安装完成
   1. ![44](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/44.png)
   
2. #### 创建语言包

   1. 打开lingguo窗口window->Lingguo,在语言栏输入语言包名字，点击Create
      1. ![1](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/1.png)
   2. 然后在Assets/Lingguo/下可以看到相应的语言包资产
      1. ![2](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/2.png)
   3. 选中Package资产，然后勾选Addressable
      1. ![4](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/4.png)
      2. ![5](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/5.png)

3. #### 为了实现语言切换，至少需要创建两个语言包,重复上述步骤创建English

4. #### 配置Lingguo

   1. 在场景中添加LingguoConfig节点，挂载LingguoConfig组件，并且添加上述步骤创建的语言包
      1. ![6](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/6.png)
      2. ![7](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/7.png)

## 字符串替换

1. #### 首先导入字符串表

   1. 选中语言包中的字符串数据库
      1. ![8](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/8.png)
   2. 点击ImportDictionary，从CSV文件导入字符串表
      1. ![9](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/9.png)
      2. CSV格式为","分割的3列的key，value，comment格式，第3列为注释，可有可无
         1. ![10](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/10.png)
         2. 两个#包裹的内容为字符串的引用，会被自动替换为相应语言的Key对应的Value
         3. 花括号包裹的内容{index}为模板参数，模板会在后面介绍

2. #### 通过Language组件绑定字符串资源

   1. 创建一个UI面板，添加一个Text组件
      
      1. ![11](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/11.png)![12](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/12.png)
   2. 在Panel节点上添加Language组件，点击"+"按钮添加一个Binder(注意，是Binder,而不是onLanguageChange事件)
      1. ![13](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/13.png)
      2. 然后将上述的Text节点拖到绑定对象字段，在成员列表中选中Text.text成员，并且在Key字段选择相应的字符串Key，如hello.lingguo(确保导入的字符串表有该Key)
         1. ![14](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/14.png)
      3. 做完上述操作，就能看到UI界面上的文字被替换为了相应语言版本
         1. ![15](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/15.png)
         2. 在LingguoConfig中，切换下当前语言为English，可以看到UI文本会立即替换为英文版本，在编辑模式下也是实时变化的
            1. ![16](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/16.png)
         3. 也可以绑定其他组件，包括**MonoBehavior**上的字符串对象，也支持**数组，Dropdown的下拉列表**类型的绑定
            1. ![17](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/17.png)![18](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/18.png)
      
   3. ##### **注意事项，为了项目容易维护，Language组件限定只能绑定本身及子节点上的对象**

3. #### 也可以通过C#特性绑定字符串

   1. 在任何C#类中，都可以在字符串变量上添加[Language]特性来绑定字符串，然后通过LanguageManager.AddLanguageScript函数将类实例添加进管理器，来实现字符串变量的自动替换
      管理器使用**弱引用**，不会影响资源释放，并且只有语言切换的时候才会触发逻辑，其他时候没有任何开销。
      1. ![19](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/19.png)
      2. 英文环境下![20](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/20.png)
      3. 中文环境下![21](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/21.png)
## Unity资源替换

目前支持**Sprite，Texture2D,Mesh,AudioClip,Material,**以及派生自**ScriptObject**的资源，可以应付绝大多数多语言资源替换需求。如有不支持的资源，可以选择使用**LanguageGameObject**组件，进行整个Prefabe的替换

   1. ####  切换Sprite资源

            1. 选中用来切换的sprite，勾选Addressable
                     1. ![22](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/22.png)
            2. 然后选中相应语言包的AssetDatabase，添加相应的资产和Key
                     1. ![24](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/24.png)![27](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/27.png)
                     2. ![28](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/28.png)![29](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/29.png)
            3. 然后通过Language，将该key和目标sprite资源进行绑定
                     1. 创建demo按钮
                              1. ![25](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/25.png)![30](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/30.png)
                     2. 使用Language组件将刚才创建的Sprite资产绑定到按钮的image成员上
                              1. ![31](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/31.png)
                     3. 此时，Sprite就能自动切换为相应的语言版本了
                              1. ![32](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/32.png)![33](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/33.png)

   2. #### 其他资产的绑定，和Sprite步骤一样

## 模板替换功能

1. #### 应用场景

   1. 当字符串是动态生成的时候，比如 “某某某”使用“某道具”攻击“某某某”，这样的文本替换需求，就得用模板的动态生成能力了。

2. #### 使用方法

   1. 在相应语言包的字符串库中添加模板字符串
      1. ![8](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/8.png)
      2. ![36](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/36.png)![37](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/37.png)
      3. 模板的格式和**C#**的**String.Format**一样，以{0}，表示第一个参数，{1}表示第二个参数...
      4. 如果字符串中需要用到花括号，请以**{{**转义 
      5. 注意例子中的模板，中文"**{0}**"使用"**{1}**"打了"**{2}**"  和英文**{0}** attack **{2}** by **{1}**，第2和第3个参数的位置，**顺序**是不一样的，这样可以解决多语言翻译中的，语素**顺序不一致**的问题
   2. 在资产目录右键菜单Create->Lingguo->Template创建模板资源(这里命名为Attack)
      1. ![34](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/34.png)
      2. 选中模板资产，将Key设为刚才添加的attack_somebody，并按照模板参数数量，添加相应的参数，在Inspector面板可以预览生成的结果
         1. ![35](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/35.png)
      3. 然后回到最开始的字符串绑定例子，通过Language组件，将绑定类型设为template，然后选择Attack模板作为key绑定目标字符串
         1. ![38](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/38.png)
         2. 切换不同语言，可以实时看到，文本会专为相应的语言模板生成
            1. ![39](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/39.png)![40](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/40.png)
      4. 在相应的C#脚本中，可以引用这个资产，就像使用Material一样，然后通过LanguageTemplate.SetParams来实时改变参数，并且自动应用到绑定对象上。
      5. 也可以脱离Language组件，直接在C#脚本中创建LanguageTemplate资产，通过Bind函数进行绑定，SetKey设置相应的字符串模板Key，SetParams设置参数实时更新绑定字符串对象

## 整体Prefabe替换

1. #### 有时候，多语言切换，有些替换无法通过上述步骤完成，比如UI大小，位置的变换，这时候可以选择Prefabe整体替换方案

   1. 制作不同语言版本的Prefabe资源
   2. 在根节点上挂载LanguageGameObject组件
   3. 设置不同语言下使用的Prefabe
      1. ![41](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/41.png)
   2. 将该Prefab添加到场景，**运行**，然后切换语言，就可以看到Prefabe的替换
      1. ![42](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/42.png)![43](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/43.png)

# 关于打包

如果项目本身就使用Addressable的，按照项目Addressable的管理策略管理多语言资源就可以。

如果不熟悉Addressable，项目本身也没有使用Addressable，那么就把多语言资源当做本地资源，打包前，以默认的配置打包Addressable:**从菜单打开Window->Asset Manager->Addressable->Groups窗口，点击Build->New Build->Default Build Script**。![47](https://raw.githubusercontent.com/treen/MarkdownPicture/main/Lingguo/47.png)



# 关于Sample

从Package包导入Sample后，需要将Assets/Samples/Lingguo/版本号/Sample目录下的AddressabeAssetsData目录移动到Assets目录下，才可以正确运行Sample
