using BepInEx;
using CasselGames.Audio;
using CasselGames.Data;
using CasselGames.Diplomatic;
using CasselGames.Diplomatic.Data;
using CasselGames.Diplomatic.UI;
using CasselGames.Encyclopedia;
using CasselGames.Input;
using CasselGames.UI;
using HarmonyLib;
using I2.Loc;
using RatopiaMod;
using Spine;
using SpecialRatizens.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using Utility.Achievement;
using Utility.Data;
using Utility.IO;
using Utility.UI;
using static PathFindMgr;
using Random = UnityEngine.Random;

namespace RatopiaMod
{
    public class CustomMOD : BaseUnityPlugin
    {
        /// <summary>
        /// 版本号
        /// </summary>
        readonly static string version = "1.5.2";

        #region 名称表

        /// <summary>
        /// 姓氏
        /// </summary>
        readonly static string[] CustomSurNames = new string[] { "赵", "钱", "孙", "李", "周", "吴", "郑", "王", "冯", "陈", "褚", "卫", "蒋", "沈", "韩", "杨", "朱", "秦", "尤", "许", "何", "吕", "施", "张", "孔", "曹", "严", "华", "金", "魏", "陶", "姜", "戚", "谢", "邹", "喻", "柏", "水", "窦", "章", "云", "苏", "潘", "葛", "奚", "范", "彭", "郎", "鲁", "韦", "昌", "马", "苗", "凤", "花", "方", "俞", "任", "袁", "柳", "酆", "鲍", "史", "唐", "费", "廉", "岑", "薛", "雷", "贺", "倪", "汤", "滕", "殷", "罗", "毕", "郝", "邬", "安", "常", "乐", "于", "时", "傅", "皮", "卞", "齐", "康", "伍", "余", "元", "卜", "顾", "孟", "平", "黄", "和", "穆", "萧", "尹", "姚", "邵", "湛", "汪", "祁", "毛", "禹", "狄", "米", "贝", "明", "臧", "计", "伏", "成", "戴", "谈", "宋", "茅", "庞", "熊", "纪", "舒", "屈", "项", "祝", "董", "梁", "杜", "阮", "蓝", "闵", "席", "季", "麻", "强", "贾", "路", "娄", "危", "江", "童", "颜", "郭", "梅", "盛", "林", "刁", "钟", "徐", "邱", "骆", "高", "夏", "蔡", "田", "樊", "胡", "凌", "霍", "虞", "万", "支", "柯", "昝", "管", "卢", "莫", "经", "房", "裘", "缪", "干", "解", "应", "宗", "丁", "宣", "贲", "邓", "郁", "单", "杭", "洪", "包", "诸", "左", "石", "崔", "吉", "钮", "龚", "程", "嵇", "邢", "滑", "裴", "陆", "荣", "翁", "荀", "羊", "於", "惠", "甄", "麴", "家", "封", "芮", "羿", "储", "靳", "汲", "邴", "糜", "松", "井", "段", "富", "巫", "乌", "焦", "巴", "弓", "牧", "隗", "山", "谷", "车", "侯", "宓", "蓬", "全", "郗", "班", "仰", "秋", "仲", "伊", "宫", "宁", "仇", "栾", "暴", "甘", "钭", "厉", "戎", "祖", "武", "符", "刘", "景", "詹", "束", "龙", "叶", "幸", "司", "韶", "郜", "黎", "蓟", "薄", "印", "宿", "白", "怀", "蒲", "邰", "从", "鄂", "索", "咸", "籍", "赖", "卓", "蔺", "屠", "蒙", "池", "乔", "阴", "欎", "胥", "能", "苍", "双", "闻", "莘", "党", "翟", "谭", "贡", "劳", "逄", "姬", "申", "扶", "堵", "冉", "宰", "郦", "雍", "舄", "璩", "桑", "桂", "濮", "牛", "寿", "通", "边", "扈", "燕", "冀", "郏", "浦", "尚", "农", "温", "别", "庄", "晏", "柴", "瞿", "阎", "充", "慕", "连", "茹", "习", "宦", "艾", "鱼", "容", "向", "古", "易", "慎", "戈", "廖", "庾", "终", "暨", "居", "衡", "步", "都", "耿", "满", "弘", "匡", "国", "文", "寇", "广", "禄", "阙", "东", "殴", "殳", "沃", "利", "蔚", "越", "夔", "隆", "师", "巩", "厍", "聂", "晁", "勾", "敖", "融", "冷", "訾", "辛", "阚", "那", "简", "饶", "空", "曾", "毋", "沙", "乜", "养", "鞠", "须", "丰", "巢", "关", "蒯", "相", "查", "後", "荆", "红", "游", "竺", "权", "逯", "盖", "益", "桓", "公", "万俟", "司马", "上官", "欧阳", "夏侯", "诸葛", "闻人", "东方", "赫连", "皇甫", "尉迟", "公羊", "澹台", "公冶", "宗政", "濮阳", "淳于", "单于", "太叔", "申屠", "公孙", "仲孙", "轩辕", "令狐", "钟离", "宇文", "长孙", "慕容", "鲜于", "闾丘", "司徒", "司空", "亓官", "司寇", "仉", "督", "子车", "颛孙", "端木", "巫马", "公西", "漆雕", "乐正", "壤驷", "公良", "拓跋", "夹谷", "宰父", "谷梁", "晋", "楚", "闫", "法", "汝", "鄢", "涂", "钦", "段干", "百里", "东郭", "南门", "呼延", "归", "海", "羊舌", "微生", "岳", "帅", "缑", "亢", "况", "后", "有", "琴", "梁丘", "左丘", "东门", "西门", "商", "牟", "佘", "佴", "伯", "赏", "南宫", "墨", "哈", "谯", "笪", "年", "爱", "阳", "佟", "第五", "言", "福", "百", "家", "姓", "终" };
        /// <summary>
        /// 女名1字
        /// </summary>
        readonly static string[] CustomPerNames_Female_One = new string[] { "真", "心", "新", "悦", "西", "兮", "楚", "初", "千", "锐", "素", "锦", "静", "镜", "斯", "舒", "瑜", "童", "楠", "景", "茗", "聿", "启", "尧", "言", "嘉", "桉", "桐", "筒", "竹", "林", "乔", "栋", "家", "翊", "松", "妍", "澜", "淇", "沐", "潆", "盈", "雨", "文", "冰", "雯", "溪", "子", "云", "汐", "潞", "淇", "妙", "涵", "灿", "夏", "珞", "煊", "晴", "彤", "诺", "宁", "恬", "钧", "灵", "昭", "琉", "晨", "曦", "南", "毓", "冉", "意", "也", "坤", "辰", "伊", "米", "安", "恩", "以", "容", "宛", "岚", "又", "衣", "亚", "悠", "允" };
        /// <summary>
        /// 女名2字
        /// </summary>
        readonly static string[] CustomPerNames_Female_Two = new string[] { "梦琪", "之雅", "之桃", "慕青", "尔岚", "初夏", "沛菡", "傲珊", "曼文", "乐菱", "惜文", "香寒", "新柔", "语蓉", "海安", "夜蓉", "涵柏", "水桃", "醉蓝", "语琴", "从彤", "傲晴", "语兰", "又菱", "碧彤", "元霜", "怜梦", "紫寒", "妙彤", "曼易", "南莲", "紫翠", "雨寒", "易烟", "如萱", "若南", "寻真", "晓亦", "向珊", "慕灵", "以蕊", "映易", "雪柳", "海云", "凝天", "沛珊", "寒云", "冰旋", "宛儿", "绿真", "晓霜", "碧凡", "夏菡", "曼香", "若烟", "半梦", "雅绿", "冰蓝", "灵槐", "平安", "书翠", "翠风", "代云", "梦曼", "幼翠", "听寒", "梦柏", "醉易", "访旋", "亦玉", "凌萱", "访卉", "怀亦", "笑蓝", "靖柏", "夜蕾", "冰夏", "梦松", "书雪", "乐枫", "念薇", "靖雁", "从寒", "觅波", "静曼", "凡旋", "以亦", "念露", "芷蕾", "千兰", "新波", "代真", "新蕾", "雁玉", "冷卉", "紫山", "千琴", "傲芙", "盼山", "怀蝶", "冰兰", "山柏", "翠萱", "问旋", "白易", "问筠", "如霜", "半芹", "丹珍", "冰彤", "亦寒", "之瑶", "冰露", "尔珍", "谷雪", "乐萱", "涵菡", "海莲", "傲蕾", "青槐", "易梦", "惜雪", "宛海", "之柔", "夏青", "亦瑶", "妙菡", "紫蓝", "幻柏", "元风", "冰枫", "访蕊", "芷蕊", "凡蕾", "凡柔", "安蕾", "天荷", "含玉", "书兰", "雅琴", "书瑶", "从安", "夏槐", "念芹", "代曼", "幻珊", "谷丝", "秋翠", "白晴", "海露", "代荷", "含玉", "书蕾", "听白", "灵雁", "雪青", "乐瑶", "含烟", "涵双", "平蝶", "雅蕊", "傲之", "灵薇", "含蕾", "从梦", "从蓉", "初丹", "听兰", "听蓉", "语芙", "夏彤", "凌瑶", "忆翠", "幻灵", "怜菡", "紫南", "依珊", "妙竹", "访烟", "怜蕾", "映寒", "友绿", "冰萍", "惜霜", "凌香", "芷蕾", "雁卉", "迎梦", "元柏", "代萱", "紫真", "千青", "凌寒", "紫安", "寒安", "怀蕊", "秋荷", "涵雁", "以山", "凡梅", "盼曼", "翠彤", "谷冬", "冷安", "千萍", "冰烟", "雅阳", "友绿", "南松", "诗云", "飞风", "寄灵", "书芹", "幼蓉", "以蓝", "笑寒", "忆寒", "秋烟", "芷巧", "水香", "映之", "醉波", "幻莲", "夜山", "芷卉", "向彤", "小玉", "幼南", "凡梦", "尔曼", "念波", "迎松", "青寒", "笑天", "涵蕾", "碧菡", "映秋", "盼烟", "忆山", "以寒", "寒香", "小凡", "代亦", "梦露", "映波", "友蕊", "寄凡", "怜蕾", "雁枫", "水绿", "曼荷", "笑珊", "寒珊", "谷南", "慕儿", "夏岚", "友儿", "小萱", "紫青", "妙菱", "冬寒", "曼柔", "语蝶", "青筠", "夜安", "觅海", "问安", "晓槐", "雅山", "访云", "翠容", "寒凡", "晓绿", "以菱", "冬云", "含玉", "访枫", "含卉", "夜白", "冷安", "灵竹", "醉薇", "元珊", "幻波", "盼夏", "元瑶", "迎曼", "水云", "访琴", "谷波", "笑白", "妙海", "紫霜", "凌旋", "孤丝", "怜寒", "凡松", "青丝", "翠安", "如天", "凌雪", "绮菱", "代云", "香薇", "冬灵", "凌珍", "沛文", "紫槐", "幻柏", "采文", "雪旋", "盼海", "映梦", "安雁", "映容", "凝阳", "访风", "天亦", "觅风", "小霜", "雪萍", "半雪", "山柳", "谷雪", "靖易", "白薇", "梦菡", "飞绿", "如波", "又晴", "友易", "香菱", "冬亦", "问雁", "海冬", "秋灵", "凝芙", "念烟", "白山", "从灵", "尔芙", "迎蓉", "念寒", "翠绿", "翠芙", "靖儿", "妙柏", "千凝", "小珍", "妙旋", "雪枫", "夏菡", "绮琴", "雨双", "听枫", "觅荷", "凡之", "晓凡", "雅彤", "孤风", "从安", "绮彤", "之玉", "雨珍", "幻丝", "代梅", "青亦", "元菱", "海瑶", "飞槐", "听露", "梦岚", "幻竹", "谷云", "忆霜", "水瑶", "慕晴", "秋双", "雨真", "觅珍", "丹雪", "元枫", "思天", "如松", "妙晴", "谷秋", "妙松", "晓夏", "宛筠", "碧琴", "盼兰", "小夏", "安容", "青曼", "千儿", "寻双", "涵瑶", "冷梅", "秋柔", "思菱", "醉波", "醉柳", "以寒", "迎夏", "向雪", "以丹", "依凝", "如柏", "雁菱", "凝竹", "宛白", "初柔", "南蕾", "书萱", "梦槐", "南琴", "绿海", "沛儿", "晓瑶", "凝蝶", "紫雪", "念双", "念真", "曼寒", "凡霜", "飞雪", "雪兰", "雅霜", "从蓉", "冷雪", "靖巧", "翠丝", "觅翠", "凡白", "乐蓉", "迎波", "丹烟", "梦旋", "书双", "念桃", "夜天", "安筠", "觅柔", "初南", "秋蝶", "千易", "安露", "诗蕊", "山雁", "友菱", "香露", "晓兰", "白卉", "语山", "冷珍", "秋翠", "夏柳", "如之", "忆南", "书易", "翠桃", "寄瑶", "如曼", "问柳", "幻桃", "又菡", "醉蝶", "亦绿", "诗珊", "听芹", "新之", "易巧", "念云", "晓灵", "静枫", "夏蓉", "如南", "幼丝", "秋白", "冰安", "秋白", "南风", "醉山", "初彤", "凝海", "紫文", "凌晴", "雅琴", "傲安", "傲之", "初蝶", "代芹", "诗霜", "碧灵", "诗柳", "夏柳", "采白", "慕梅", "乐安", "冬菱", "紫安", "宛凝", "雨雪", "易真", "安荷", "静竹", "代柔", "丹秋", "绮梅", "依白", "凝荷", "幼珊", "忆彤", "凌青", "之桃", "芷荷", "听荷", "代玉", "念珍", "梦菲", "夜春", "千秋", "白秋", "谷菱", "飞松", "初瑶", "惜灵", "梦易", "新瑶", "曼梅", "碧曼", "友瑶", "雨兰", "夜柳", "芷珍", "含芙", "夜云", "依萱", "凝雁", "以莲", "安南", "幼晴", "尔琴", "飞阳", "白凡", "沛萍", "雪瑶", "向卉", "采文", "乐珍", "寒荷", "觅双", "白桃", "安卉", "迎曼", "盼雁", "乐松", "涵山", "问枫", "以柳", "含海", "翠曼", "忆梅", "涵柳", "海蓝", "晓曼", "代珊", "忆丹", "静芙", "绮兰", "梦安", "紫丝", "千雁", "凝珍", "香萱", "梦容", "冷雁", "飞柏", "天真", "翠琴", "寄真", "秋荷", "代珊", "初雪", "雅柏", "怜容", "如风", "南露", "紫易", "冰凡", "海雪", "语蓉", "碧玉", "语风", "凝梦", "从雪", "白枫", "傲云", "白梅", "念露", "慕凝", "雅柔", "盼柳", "半青", "从霜", "怀柔", "怜晴", "夜蓉", "代双", "以南", "若菱", "芷文", "南晴", "梦寒", "初翠", "灵波", "问夏", "惜海", "亦旋", "沛芹", "幼萱", "白凝", "初露", "迎海", "绮玉", "凌香", "寻芹", "秋柳", "尔白", "映真", "含雁", "寒松", "寻雪", "青烟", "问蕊", "灵阳", "雪巧", "丹萱", "凡双", "孤萍", "紫菱", "寻凝", "傲柏", "傲儿", "友容", "灵枫", "尔丝", "曼凝", "若蕊", "问丝", "思枫", "水卉", "问梅", "念寒", "诗双", "翠霜", "夜香", "寒蕾", "凡阳", "冷玉", "平彤", "语薇", "幻珊", "紫夏", "凌波", "芷蝶", "丹南", "之双", "凡波", "思雁", "白莲", "从菡", "如容", "采柳", "沛岚", "惜儿", "夜玉", "水儿", "半凡", "语海", "听莲", "幻枫", "念柏", "冰珍", "思山", "凝蕊", "天玉", "思萱", "向梦", "笑南", "夏旋", "之槐", "元灵", "以彤", "采萱", "巧曼", "绿兰", "平蓝", "问萍", "绿蓉", "靖柏。迎蕾", "碧曼", "思卉", "白柏", "妙菡", "怜阳", "雨柏", "雁菡", "梦之", "又莲", "乐荷", "寒天", "凝琴", "书南", "映天", "白梦", "初瑶", "平露", "含巧", "慕蕊", "半莲", "醉卉", "天菱", "青雪", "雅旋", "巧荷", "飞丹", "若灵", "尔云", "幻天", "诗兰", "青梦", "海菡", "灵槐", "忆秋", "寒凝", "凝芙", "绮山", "静白", "尔蓉", "尔冬", "映萱", "白筠", "冰双", "访彤", "绿柏", "夏云", "笑翠", "晓灵", "含双", "盼波", "以云", "怜翠", "雁风", "之卉", "平松", "问儿", "绿柳", "如蓉", "曼容", "天晴", "丹琴", "惜天", "寻琴", "依瑶", "涵易", "忆灵", "从波", "依柔", "问兰", "山晴", "怜珊", "之云", "飞双", "傲白", "沛春", "雨南", "梦之", "笑阳", "代容", "友琴", "雁梅", "友桃", "从露", "语柔", "傲玉", "觅夏", "晓蓝", "新晴", "雨莲", "凝旋", "绿旋", "幻香", "觅双", "冷亦", "忆雪", "友卉", "幻翠", "靖柔", "寻菱", "丹翠", "安阳", "雅寒", "惜筠", "尔安", "雁易", "飞瑶", "夏兰", "沛蓝", "静丹", "山芙", "笑晴", "新烟", "笑旋", "雁兰", "凌翠", "秋莲", "书桃", "傲松", "语儿", "映菡", "初曼", "听云", "初夏", "雅香", "语雪", "初珍", "白安", "冰薇", "诗槐", "冷玉", "冰巧", "之槐", "夏寒", "诗筠", "新梅", "白曼", "安波", "从阳", "含桃", "曼卉", "笑萍", "晓露", "寻菡", "沛白", "平灵", "水彤", "安彤", "涵易", "乐巧", "依风", "紫南", "亦丝", "易蓉", "紫萍", "惜萱", "诗蕾", "寻绿", "诗双", "寻云", "孤丹", "谷蓝", "山灵", "幻丝", "友梅", "从云", "雁丝", "盼旋", "幼旋", "尔蓝", "沛山", "代丝", "觅松", "冰香", "依玉", "冰之", "妙梦", "以冬", "曼青", "冷菱", "雪曼", "安白", "千亦", "凌蝶", "又夏", "南烟。靖易", "沛凝", "翠梅", "书文", "雪卉", "乐儿", "傲丝", "安青", "初蝶", "寄灵", "惜寒", "雨竹", "冬莲", "绮南", "翠柏", "平凡", "亦玉", "孤兰", "秋珊", "新筠", "半芹", "夏瑶", "念文", "晓丝", "涵蕾", "雁凡", "谷兰", "灵凡", "凝云", "曼云", "丹彤", "南霜", "夜梦", "从筠", "雁芙", "语蝶", "依波", "晓旋", "念之", "盼芙", "曼安", "采珊", "初柳", "迎天", "曼安", "南珍", "妙芙", "语柳", "含莲", "晓筠", "夏山", "尔容", "念梦", "傲南", "问薇", "雨灵", "凝安", "冰海", "初珍", "宛菡", "冬卉", "盼晴", "冷荷", "寄翠", "幻梅", "如凡", "语梦", "易梦", "千柔", "向露", "梦玉", "傲霜", "依霜", "灵松", "诗桃", "书蝶", "冰蝶", "山槐", "以晴", "友易", "梦桃", "香菱", "孤云", "水蓉", "雅容", "飞烟", "雁荷", "代芙", "醉易", "夏烟", "依秋", "依波", "紫萱", "涵易", "忆之", "幻巧", "水风", "安寒", "白亦", "怜雪", "听南", "念蕾", "梦竹", "千凡", "寄琴", "采波", "元冬", "思菱", "平卉", "笑柳", "雪卉", "谷梦", "绿蝶", "飞荷", "平安", "孤晴", "芷荷", "曼冬", "尔槐", "以旋", "绿蕊", "初夏", "依丝", "怜南", "千山", "雨安", "水风", "寄柔", "幼枫", "凡桃", "新儿", "夏波", "雨琴", "静槐", "元槐", "映阳", "飞薇", "小凝", "映寒", "傲菡", "谷蕊", "笑槐", "飞兰", "笑卉", "迎荷", "元冬", "书竹", "半烟", "绮波", "小之", "觅露", "夜雪", "寒梦", "尔风", "白梅", "雨旋", "芷珊", "山彤", "尔柳", "沛柔", "灵萱", "沛凝", "白容", "乐蓉", "映安", "依云", "映冬", "凡雁", "梦秋", "醉柳", "梦凡", "若云", "元容", "怀蕾", "灵寒", "天薇", "白风", "访波", "亦凝", "易绿", "夜南", "曼凡", "亦巧", "青易。冰真", "白萱", "友安", "诗翠", "雪珍", "海之", "小蕊", "又琴", "香彤", "语梦", "惜蕊", "迎彤", "沛白", "雁山", "易蓉", "雪晴", "诗珊", "冰绿", "半梅", "笑容", "沛凝", "念瑶", "如冬", "向真", "从蓉", "亦云", "向雁", "尔蝶", "冬易", "丹亦", "夏山", "醉香", "盼夏", "孤菱", "安莲", "问凝", "冬萱", "晓山", "雁蓉", "梦蕊", "山菡", "南莲", "飞双", "凝丝", "思萱", "怀梦", "雨梅", "冷霜", "向松", "迎丝", "迎梅", "听双", "山蝶", "夜梅", "醉冬", "雨筠", "平文", "青文", "半蕾", "幼菱", "寻梅", "含之", "香之", "含蕊", "亦玉", "靖荷", "碧萱", "寒云", "向南", "书雁", "怀薇", "思菱", "忆文", "若山", "向秋", "凡白", "绮烟", "从蕾", "天曼", "又亦", "依琴", "曼彤", "沛槐", "又槐", "元绿", "安珊", "夏之", "易槐", "宛亦", "白翠", "丹云", "问寒", "易文", "傲易", "青旋", "思真", "妙之", "半双", "若翠", "初兰", "怀曼", "惜萍", "初之", "宛丝", "幻儿", "千风", "天蓉", "雅青", "寄文", "代天", "惜珊", "向薇", "冬灵", "惜芹", "凌青", "谷芹", "雁桃", "映雁", "书兰", "寄风", "访烟", "绮晴", "傲柔", "寄容", "以珊", "紫雪", "芷容", "书琴", "寻桃", "涵阳", "怀寒", "易云", "采蓝", "代秋", "惜梦", "尔烟", "谷槐", "怀莲", "涵菱", "水蓝", "访冬", "半兰", "又柔", "冬卉", "安双", "冰岚", "香薇", "语芹", "静珊", "幻露", "访天", "静柏", "凌丝", "小翠", "雁卉", "访文", "凌文", "芷云", "思柔", "巧凡", "慕山", "依云", "千柳", "从凝", "安梦", "香旋", "映天", "安柏", "平萱", "以筠", "忆曼", "新竹", "绮露", "觅儿", "碧蓉", "白竹", "飞兰", "曼雁", "雁露", "凝冬", "含灵", "初阳", "海秋", "冰双", "绿兰", "盼易", "思松", "梦山", "友灵", "绿竹", "灵安", "凌柏", "秋柔", "又蓝", "尔竹", "天蓝", "青枫", "问芙", "语海", "灵珊", "凝丹", "小蕾", "迎夏", "水之", "飞珍", "冰夏", "亦竹", "飞莲", "海白", "元蝶", "芷天", "怀绿", "尔容", "元芹", "若云", "寒烟", "听筠", "采梦", "凝莲", "元彤", "觅山", "代桃", "冷之", "盼秋", "秋寒", "慕蕊", "海亦", "初晴", "巧蕊", "听安", "芷雪", "以松", "梦槐", "寒梅", "香岚", "寄柔", "映冬", "孤容", "晓蕾", "安萱", "听枫", "夜绿", "雪莲", "从丹", "碧蓉", "绮琴", "雨文", "幼荷", "青柏", "初蓝", "忆安", "盼晴", "寻冬", "雪珊", "梦寒", "迎南", "如彤", "采枫", "若雁", "翠阳", "沛容", "幻翠", "山兰", "芷波", "雪瑶", "寄云", "慕卉", "冷松", "涵梅", "书白", "乐天", "雁卉", "宛秋", "傲旋", "新之", "凡儿", "夏真", "静枫", "乐双", "白玉", "问玉", "寄松", "丹蝶", "元瑶", "冰蝶", "访曼", "代灵", "芷烟", "白易", "尔阳", "怜烟", "平卉", "丹寒", "访梦", "绿凝", "冰菱", "语蕊", "思烟", "忆枫", "映菱", "凌兰", "曼岚", "若枫", "傲薇", "凡灵", "乐蕊", "秋灵", "谷槐", "觅云" };
        /// <summary>
        /// 男名1字
        /// </summary>
        readonly static string[] CustomPerNames_Male_One = new string[] { "勇", "贵", "广", "威", "义", "翔", "信", "庆", "宏", "远", "进", "飞", "青", "胜", "博", "平", "宏", "勇", "轩", "武", "崇", "忠", "华", "丰", "慧", "和", "冠", "凌", "威", "豪", "清", "鹤", "杰", "平", "才", "海", "峰", "刚", "宁", "华", "全", "南", "忌", "羽", "遥", "誉", "颇", "靖", "铭", "琛", "川", "承", "司", "斯", "宗", "骁", "聪", "在", "钩", "锦", "铎", "楚", "铮", "钦", "则", "楠", "景", "茗", "聿", "启", "尧", "言", "嘉", "桉", "桐", "筒", "竹", "林", "乔", "栋", "家", "翊", "松", "清", "澈", "泫", "浚", "润", "泽", "向", "凡", "文", "浦", "洲", "珩", "玄", "洋", "淮", "雨", "子", "云", "卓", "昱", "南", "晨", "知", "宁", "年", "易", "晗", "炎", "焕", "哲", "煦", "旭", "明", "阳", "朗", "典", "辰", "宸", "野", "安", "为", "亦，围", "岚", "也", "以", "延", "允", "容", "恩", "衡", "宇", "硕", "已" };
        /// <summary>
        /// 男名2字
        /// </summary>
        readonly static string[] CustomPerNames_Male_Two = new string[] { "沐泽", "春和", "旭泽", "扶风", "廷辰", "彦盛", "彦成", "明新", "墨池", "西岭", "顾北", "苏俊", "汶皓", "瑞岚", "津星", "浩灏", "晨诺", "宇辰", "亦晨", "承洲", "容隐", "云深", "凌白", "煜爵", "铭迅", "睿度", "浩硕", "浩嘉", "川乐", "奕衍", "铭程", "涛晨", "轩鸿", "德烁", "彬韦", "玮信", "奇羽", "展宸", "司深", "修远", "以修", "羽辰", "浩宇", "思辰", "博彤", "艺诣", "晓博", "俊亭", "海立", "富荣", "永潼", "羽洋", "泽旷", "柏年", "君海", "煜泽", "瀚龙", "文斌", "宏玮", "昱铭", "颢洋", "泽鸿", "嘉阳", "彦清", "健蔚", "豪萱", "语祺", "成飞", "晓泽", "宇旭", "志轩", "文勉", "立凡", "凌翔", "一帆", "泽卫", "玮略", "泽风", "诚荣", "浩初", "志尧", "嘉研", "博艺", "睿思", "敏达", "新理", "绍志", "沁安", "文德", "海峰", "昊苍", "俊天", "玉生", "濂道", "尧泽", "慕宇", "晏温", "奕宸", "楚仁", "喻之", "思墨", "辰彦", "盛利", "连余", "卓晗", "烨熙", "宇伦", "逸尚", "古易", "玮余", "贤辉", "泞恒", "岑蕴", "泽东", "宁谚", "昱腾", "加惠", "琙星", "祺协", "健坤", "业熠", "木白", "言启", "贤志", "峻渝", "禹洵", "意寐", "宗灿", "泽翔", "星梓", "子衿", "明元", "勤林", "川博", "雄熙", "哄鹂", "赫恒", "至初", "忠华", "溪依", "皓旭", "琛项", "誉凯", "震超", "嘉浓", "鼎炫", "新达", "淑拉", "凌宵", "淇乐", "远棕", "镜烁", "炀应", "谷奇", "尧宸", "盛熠", "玮淞", "昌童", "泽帛", "又羽", "煜云", "葛格", "弘羽", "淇恒", "辰振", "博恩", "颂东", "伯宇", "诵昀", "辉彤", "牧论", "宏容", "利生", "翔明", "熠名", "新宇", "志玮", "雅珊", "浩星", "飞铮", "景泓", "炳鑫", "光雷", "永铎", "军钢", "尊秦", "卓乐", "韦祯", "亦毓", "林苇", "新睿", "金恒", "金恒", "薄斗", "祚皓", "广宏", "谷易", "砾君", "正好", "昊吉", "得庸", "忆轩", "彦友", "韵辰", "畅阳", "兴宸", "颢音", "郎旻", "杰钢", "登易", "锴淇", "麟龙", "纡萱", "金伟", "然炜", "雨斌", "昊东", "钰宏", "瑞友", "依忠", "金钧", "依忠", "金钧", "留刚", "敬刚", "晨霏", "倚云", "斯华", "冠宸", "振宇", "远齐", "明力", "赫绚", "迪仁", "承天", "谷明", "宏枋", "为远", "晋智", "圣锦", "耘邺", "阁栋", "迷途", "金承", "梓承", "寒龙", "钧信", "思峰", "铁将", "乐瀚", "远利", "昌达", "薄蔼", "灏勤", "齐辛", "修永", "朗烁", "铮伟", "入邗", "枳旭", "子喧", "逸丰", "航豪", "航风", "昕孜", "磊泓", "信志", "辰谦", "跃杜", "谦胥", "延彦", "思臣", "果贤", "岂畅", "波丞", "秋深", "贞谦", "琦岳", "新暄", "日益", "天萧", "雨志", "钰森", "晟棋", "宫祥", "韬尊", "弘谦", "依莫", "松坤", "乔羽", "万松", "弘驿", "竹菱", "轩贤", "玮辉", "慕辰", "轩全", "荷容", "岳仲", "哲凯", "格毅", "攽逸", "铭良", "彭逸", "凯新", "贤伟", "懿泽", "和余", "如绚", "宁弈", "玉续", "茂峰", "少檑", "苑家", "峻皇", "明远", "昌逸", "敬实", "浚坤", "京铮", "昊斌", "道尊", "远华", "全球", "宜灏", "弥硕", "玟学", "梁明", "宸晏", "江隆", "敬文", "棰岩", "弈昌", "瑾海", "炳炜", "喜海", "琈韩", "钰永", "稀辰", "明伟", "孟赫", "习知", "业浩", "浩谦", "兴彬", "中宁", "春业", "嘉烁", "瀚默", "凤瑗", "熙尧", "宇衍", "星野", "基彪", "基逸", "隽杜", "书昌", "皓澎", "洪冰", "清峰", "绩含", "栋峰", "思炎", "轩坤", "雪健", "宗嘉", "鄞涵", "师雁", "致安", "彦兆", "敬理" };

        #endregion

        #region 官方引用

        /// <summary>
        /// 游戏编辑管理器
        /// </summary>
        static EditorOptionMgr EditorMgr { get { return GameMgr.Instance._EditorMgr; } }
        /// <summary>
        /// 作弊管理器
        /// </summary>
        static CheatMgr CheatMgr { get { return DebugMgr.Instance._CheatMgr; } }
        /// <summary>
        /// 相机管理器
        /// </summary>
        static CameraMgr CameraMgr { get { return GameMgr.Instance._CamMgr; } }
        /// <summary>
        /// 系统管理器
        /// </summary>
        static SystemMgr SystemMgr { get { return GameMgr.Instance._SysMgr; } }
        /// <summary>
        /// 单位管理器
        /// </summary>
        static T_UnitMgr UnitMgr { get { return GameMgr.Instance._T_UnitMgr; } }
        /// <summary>
        /// 池管理器
        /// </summary>
        static PoolMgr PoolMgr { get { return GameMgr.Instance._PoolMgr; } }
        /// <summary>
        /// 统计管理器
        /// </summary>
        static StatisticsMgr SttMgr { get { return GameMgr.Instance._SttMgr; } }
        /// <summary>
        /// 数据管理器
        /// </summary>
        static DB_Mgr DBMgr { get { return GameMgr.Instance._DB_Mgr; } }
        /// <summary>
        /// 存储管理器
        /// </summary>
        static LoadMgr LoadMgr { get { return GameMgr.Instance._LoadMgr; } }
        static EconomicMgr EcoMgr { get { return GameMgr.Instance._EcoMgr; } }
        /// <summary>
        /// 外交管理器
        /// </summary>
        static DiplomaticMgr DiplomaticMgr { get { return GameMgr.Instance.DiplomaticMgr; } }
        /// <summary>
        /// 瓦片管理器
        /// </summary>
        static TileMgr TileMgr { get { return GameMgr.Instance._TileMgr; } }
        /// <summary>
        /// 建筑管理器
        /// </summary>
        static BuildingMgr BuildingMgr { get { return GameMgr.Instance._BuildingMgr; } }
        /// <summary>
        /// 天气管理器
        /// </summary>
        static WeatherMgr WeatherMgr { get { return GameMgr.Instance._WeatherMgr; } }
        /// <summary>
        /// 游戏数据管理器
        /// </summary>
        static PlayDataMgr PlayDataMgr { get { return PlayDataMgr.Instance; } }
        /// <summary>
        /// 绘图管理器
        /// </summary>
        static PallateMgr PallateMgr { get { return DebugMgr.Instance._PallateMgr; } }
        /// <summary>
        /// 寻路管理器
        /// </summary>
        static PathFindMgr PathFindMgr { get { return GameMgr.Instance._PathFindMgr; } }

        /// <summary>
        /// 繁荣界面
        /// </summary>
        static ProsperityUI ProsperityUI { get { return GameMgr.Instance._ProsperityUI; } }
        /// <summary>
        /// 法典界面
        /// </summary>
        static PolicyUI PolicyUI { get { return GameMgr.Instance._PolicyUI; } }
        /// <summary>
        /// 中间警告界面
        /// </summary>
        static CenterAlarmUI CenterAlarmUI { get { return GameMgr.Instance._CenterAlarmUI; } }
        /// <summary>
        /// 建筑界面
        /// </summary>
        static ConstructUI ConstructUI { get { return GameMgr.Instance._ConstructUI; } }
        /// <summary>
        /// 市民信息界面
        /// </summary>
        static CitizenInfoUI CitizenInfoUI { get { return GameMgr.Instance._CitizenInfoUI; } }
        /// <summary>
        /// 市民阶级信息界面
        /// </summary>
        static StatusCitizenInfoUI StatusCitizenInfoUI { get { return GetPrivateValue<StatusCitizenInfoUI>(CitizenInfoUI, "_statusCitizenInfoUI"); } }
        /// <summary>
        /// 移民界面
        /// </summary>
        static CitizenCaveUI CitizenCaveUI { get { return GameMgr.Instance._CCUI; } }

        /// <summary>
        /// 繁荣等级
        /// </summary>
        static int ProsperityLevel { get { return ProsperityUI.m_Level; } }
        /// <summary>
        /// 金币
        /// </summary>
        static float CountryGold { get { return EcoMgr.m_Gold; } set { EcoMgr.m_Gold = value; EcoMgr.m_GoldUI.TxtUpdate(); } }
        /// <summary>
        /// 外交数据
        /// </summary>
        static DiplomaticData DiplomaticData { get { return GetPrivateValue<DiplomaticData>(DiplomaticMgr, "_data"); } }
        /// <summary>
        /// 外交城市数据
        /// </summary>
        static Dictionary<Vector2Int, DiplomaticCountryData> CountryDatas { get { return GetPrivateValue<Dictionary<Vector2Int, DiplomaticCountryData>>(DiplomaticData, "_mapDic"); } }
        /// <summary>
        /// Sprite图片字典
        /// </summary>
        static Dictionary<string, Sprite> DicSprits { get { return GetPrivateValue<Dictionary<string, Sprite>>(Func.Instance, "Dic_Resource"); } }
        /// <summary>
        /// 所有居民
        /// </summary>
        static List<T_Citizen> Citizens { get { return UnitMgr.List_Citizen; } }
        /// <summary>
        /// 女王
        /// </summary>
        static T_Queen Queen { get { return UnitMgr.m_Queen; } }

        /// <summary>
        /// 游戏暂停中
        /// </summary>
        static bool GameIsPaused { get { return SystemMgr != null && SystemMgr.IsGamePause(); } }
        /// <summary>
        /// 当前游戏数据
        /// </summary>
        static D_Data GameData { get { return PlayDataMgr.Instance.m_GameData; } }

        #endregion

        /// <summary>
        /// 导出数据
        /// </summary>
        readonly static bool OutPutDatas = false;
        /// <summary>
        /// 数据路径
        /// </summary>
        readonly static string DataPath = "GameDatas/";
        /// <summary>
        /// 自定义数据路径
        /// </summary>
        static string CustomDataPath = "CustomSetting_Data/";
        /// <summary>
        /// 配置文件名称
        /// </summary>
        readonly static string SettingsFileName = "CustomSettings.json";
        /// <summary>
        /// 好特征名称列表
        /// </summary>
        readonly static List<string> GoodCharacteristicNames = new List<string>
        {
            "Strong", "Diligent", "Intelligent", "Quick", "Thin", "Frugal", "Quiet", "Clean", "Skilful", "Porter",
            "Elitism", "Collectivism", "Combative", "Progressive", "Dependent", "Optimistic", "Believer", "Immune", "Carnivore", "Gutsy", "Plantlover", "Huntlover", "Mininglover", "VicariousSatisfaction", "Craftsmanship", "Extrafat", "HumbleContentment", "BalancedLifestyle", "EliteAspirant", "Runaway"
        };

        /// <summary>
        /// 屏幕标准尺寸
        /// </summary>
        static Vector2 ScreenStandardSize = new Vector2(1920f, 1080f);
        /// <summary>
        /// MOD设置界面尺寸
        /// </summary>
        static Vector2 ModSetUISize = new Vector2(800f, 600f);
        /// <summary>
        /// 皮肤设置界面尺寸
        /// </summary>
        static Vector2 SkinSetUISize = new Vector2(450f, 400f);

        static MonoBehaviour mono;

        /// <summary>
        /// 程序集
        /// </summary>
        static Assembly assembly;

        /// <summary>
        /// 由独立 BepInEx 5 入口配置。此兼容核心自身不会注册插件或自动安装补丁。
        /// </summary>
        public static void ConfigureSpecialRatizens(bool enabled, string dataRoot)
        {
            if (string.IsNullOrWhiteSpace(dataRoot))
                throw new ArgumentException("特殊鼠鼠数据目录不能为空", nameof(dataRoot));

            CustomDataPath = Path.GetFullPath(dataRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            CustomSettings.CustomSpecialUnit = enabled;
            assembly = Assembly.Load("Assembly-CSharp");
            mono = null;
        }

        /// <summary>
        /// 读档完成后仅重建特殊鼠鼠会话状态与特性效果。
        /// </summary>
        public static void SpecialRatizensSessionLoaded()
        {
            ResetSpecialRatizensSession();
            LoadCitizenDatas();
            if (ActiveCustomSpecialUnit)
                UpdateAllUsedSpecialEffects();
        }

        /// <summary>
        /// 场景切换或插件卸载时清理所有非持久运行时引用。
        /// </summary>
        public static void ResetSpecialRatizensSession()
        {
            SpecialCitizens.Clear();
            SpecialCitizenSkins.Clear();
            CitizenCustomSkins.Clear();
            OpenedCitizenInfo = null;
            OpenedSpcialCitizen = false;
            EditingCustomSkins = null;
            EditingCustomSkinIndex.Clear();
            specialUnit = null;
            foreach (CustomSpecialUnit unit in CustomSpecialUnitDatas.Values)
            {
                unit.isUsed = false;
                unit.pdr_C = 0;
            }
            foreach (CustomCharInfo info in CustomCharInfo.Values)
                info.ClearUser();
            preValueDic.Clear();
            CountryCommercialityDatas.Clear();
            SuperElecLine = null;
            AMJ7_PDI = 0;
            priceIsUpdateBySS = -1;
        }

        #region 自定义设置

        /// <summary>
        /// 自定义设置
        /// </summary>
        static CustomSettings CustomSettings = new CustomSettings();

        /// <summary>
        /// 加载自定义设置
        /// </summary>
        static void LoadCustomSettings()
        {
            if (!BaseCommand.LoadObjectByJson(SettingsFileName, out CustomSettings))
            {
                CustomSettings = new CustomSettings();

                return;
            }
        }

        /// <summary>
        /// 保存自定义设置
        /// </summary>
        static void SaveCustomSettings()
        {
            BaseCommand.SaveObjectToJson(SettingsFileName, CustomSettings);
        }

        /// <summary>
        /// 加速值
        /// </summary>
        static float accelerateValue = 1.25f;
        /// <summary>
        /// 启用游戏时间加速
        /// </summary>
        static bool ActiveAddTimeScale
        {
            get { return CustomSettings.AddTimeScale; }
            set
            {
                CustomSettings.AddTimeScale = value;

                SystemMgr.SetTimeScale(value ? accelerateValue : 1f);
            }
        }
        /// <summary>
        /// 启用时间流速减半
        /// </summary>
        static bool ActiveTimeUpdateReducedSpeed { get { return CustomSettings.TimeUpdateReducedSpeed; } set { CustomSettings.TimeUpdateReducedSpeed = value; } }
        /// <summary>
        /// 启用人口限制翻倍
        /// </summary>
        static bool ActiveAddPopLimit
        {
            get { return CustomSettings.AddPopLimit; }
            set
            {
                CustomSettings.AddPopLimit = value;

                if (ActivePoPUnLimit)
                    return;

                ProspertiyInfo info;

                //为所有繁荣等级更新人口上限
                for (int i = 0; i < DBMgr.List_ProsperityDB.Count; i++)
                {
                    info = DBMgr.List_ProsperityDB[i];

                    info.Pop = ProsperityDB[i].Pop * (value ? 2 : 1);
                }

                //Debug.Log($"启用更多人口 {value}");

                UpdateCitizenTxt();
            }
        }
        /// <summary>
        /// 启用无人机数量翻倍
        /// </summary>
        static bool ActiveAddDroneCount
        {
            get { return CustomSettings.AddDroneCount; }
            set
            {
                CustomSettings.AddDroneCount = value;

                //为所有无人机库更新无人机数量
                for (int i = 0; i < BuildingMgr.List_Building.Count; i++)
                {
                    Building_MiningCompany company = BuildingMgr.List_Building[i] as Building_MiningCompany;

                    if (company == null)
                        continue;

                    ResetMiningCompanyDroneCount(company, value ? 6 : 3);
                }
            }
        }
        /// <summary>
        /// 启用机器人数量翻倍
        /// </summary>
        static bool ActiveAddBotCount
        {
            get { return CustomSettings.AddBotCount; }
            set
            {
                CustomSettings.AddBotCount = value;

                DBMgr.Dic_BuildDB[BuildingName.RatronFactory].EffectValue2_Num = value ? DefaultBotCount * 2 : DefaultBotCount;
            }
        }
        /// <summary>
        /// 启用全正面特性
        /// </summary>
        static bool ActiveOnlyGoodCharacteristic { get { return CustomSettings.OnlyGoodCharacteristic; } set { CustomSettings.OnlyGoodCharacteristic = value; } }
        /// <summary>
        /// 启用猎人受伤减半
        /// </summary>
        static bool ActiveHunterBeAttackGetHalfDamage { get { return CustomSettings.HunterBeAttackGetHalfDamage; } set { CustomSettings.HunterBeAttackGetHalfDamage = value; } }
        /// <summary>
        /// 启用经验提高50%
        /// </summary>
        static bool ActiveMoreExp { get { return CustomSettings.MoreExp; } set { CustomSettings.MoreExp = value; } }
        /// <summary>
        /// 启用力量影响负重翻倍
        /// </summary>
        static bool ActiveMoreCapacityByPower { get { return CustomSettings.MoreCapacityByPower; } set { CustomSettings.MoreCapacityByPower = value; } }
        /// <summary>
        /// 启用仓库容量翻倍
        /// </summary>
        static bool ActiveAddStorageCapacity
        {
            get { return CustomSettings.AddStorageCapacity; }
            set
            {
                CustomSettings.AddStorageCapacity = value;

                UpdateStorageCapacity(ActiveShareStorage);
            }
        }
        /// <summary>
        /// 启用默认选中已拾起的物品
        /// </summary>
        static bool ActiveDefaultSelectSameItem { get { return CustomSettings.DefaultSelectSameItem; } set { CustomSettings.DefaultSelectSameItem = value; } }
        /// <summary>
        /// 启用按住左键连续拾取物品
        /// </summary>
        static bool ActiveContinuousTakeOutItems { get { return CustomSettings.ContinuousTakeOutItems; } set { CustomSettings.ContinuousTakeOutItems = value; } }
        /// <summary>
        /// 启用行动不丢弃拾取物品
        /// </summary>
        static bool ActiveActionNoDropGatheredItems { get { return CustomSettings.ActionNoDropGatheredItems; } set { CustomSettings.ActionNoDropGatheredItems = value; } }
        /// <summary>
        /// 启用特殊单位
        /// </summary>
        static bool ActiveCustomSpecialUnit
        {
            get { return CustomSettings.CustomSpecialUnit; }
            set
            {
                CustomSettings.CustomSpecialUnit = value;

                UpdateAllUsedSpecialEffects();
            }
        }
        /// <summary>
        /// 启用蓝图无需材料
        /// </summary>
        //static bool ActiveBluePrintNoNeedRes { get { return CustomSettings.BluePrintNoNeedRes; } set { CustomSettings.BluePrintNoNeedRes = value; } }
        /// <summary>
        /// 启用贸易详细信息
        /// </summary>
        static bool ActiveSheetMoreInfo { get { return CustomSettings.SheetMoreInfo; } set { CustomSettings.SheetMoreInfo = value; } }
        /// <summary>
        /// 启用友方掉落无伤
        /// </summary>
        static bool ActiveDropNoDamgeWithOurTeam { get { return CustomSettings.DropNoDamgeWithOurTeam; } set { CustomSettings.DropNoDamgeWithOurTeam = value; } }
        /// <summary>
        /// 启用共享床位
        /// </summary>
        static bool ActiveShareHome { get { return CustomSettings.ShareHome; } set { CustomSettings.ShareHome = value; } }
        /// <summary>
        /// 启用自定义姓名
        /// </summary>
        static bool ActiveCustomNames { get { return CustomSettings.CustomNames; } set { CustomSettings.CustomNames = value; } }

        /// <summary>
        /// 设置移民性别
        /// </summary>
        static int NewCitizenGenderLimit { get { return CustomSettings.NewCitizenGenderLimit; } set { CustomSettings.NewCitizenGenderLimit = value; } }
        /// <summary>
        /// 启用贸易通知
        /// </summary>
        static bool ActiveTradeResultMessage { get { return CustomSettings.TradeResultMessage; } set { CustomSettings.TradeResultMessage = value; } }
        /// <summary>
        /// 启用乌托邦模式
        /// </summary>
        static bool ActiveUtopiaMode
        {
            get
            {
                return CustomSettings.UtopiaMode;
            }
            set
            {
                CustomSettings.UtopiaMode = value;

                //更新所有物品物价
                UpdateTileDBToUtopiaMode(value);

                //更新建筑建造费用、工资
                UpdateBuildDBToUtopiaMode(value);

                //更新权限
                //UpdateDesireToUtopiaMode(value);

                //Debug.Log($"共 {PolicyUI.m_ComPannel.List_Act.Count} 商法");

                //更新法律影响
                PolicyUI.m_ComPannel.ComLawUpdate();
            }
        }
        /// <summary>
        /// 和平模式
        /// </summary>
        static bool ActiveSafeMode { get { return CustomSettings.SafeMode; } set { CustomSettings.SafeMode = value; } }
        /// <summary>
        /// 启用敌方死亡掉落
        /// </summary>
        static bool ActiveEnemyDeadthDrop { get { return CustomSettings.EnemyDeadthDrop; } set { CustomSettings.EnemyDeadthDrop = value; } }
        /// <summary>
        /// 启用更多种植
        /// </summary>
        static bool ActiveMorePlantingPlants
        {
            get { return CustomSettings.MorePlantingPlants; }
            set
            {
                CustomSettings.MorePlantingPlants = value;

                SetMorePlantingDB(value);
            }
        }
        /// <summary>
        /// 启用无限人口
        /// </summary>
        static bool ActivePoPUnLimit
        {
            get { return CustomSettings.PoPUnLimit; }
            set
            {
                ProspertiyInfo info;

                //为所有繁荣等级更新人口上限
                for (int i = 0; i < DBMgr.List_ProsperityDB.Count; i++)
                {
                    info = DBMgr.List_ProsperityDB[i];

                    info.Pop = value ? int.MaxValue : ProsperityDB[i].Pop * (ActiveAddPopLimit ? 2 : 1);
                }

                CustomSettings.PoPUnLimit = value;

                UpdateCitizenTxt();
            }
        }
        /// <summary>
        /// 餐桌优化
        /// </summary>
        //static bool ActiveOptimizeFoodTable { get { return CustomSettings.OptimizeFoodTable; } set { CustomSettings.OptimizeFoodTable = value; } }
        /// <summary>
        /// 启用AI优化
        /// </summary>
        static bool ActiveOptimizeAI
        {
            get { return CustomSettings.OptimizeAI; }
            set
            {
                CustomSettings.OptimizeAI = value;

                if (value)
                    UpdateDesireCut();
                else
                    SetDesireCut(true);
            }
        }
        /// <summary>
        /// 启用物品存取寻路优化
        /// </summary>
        static bool ActiveOptimizeBuyAndSellPathFind { get { return CustomSettings.OptimizeBuyAndSellPathFind; } set { CustomSettings.OptimizeBuyAndSellPathFind = value; } }
        /// <summary>
        /// 启用食物日用品寻路优化
        /// </summary>
        static bool ActiveOptimizeFoodAndLifePathFind { get { return CustomSettings.OptimizeFoodAndLifePathFind; } set { CustomSettings.OptimizeFoodAndLifePathFind = value; } }
        /// <summary>
        /// 启用娱乐卫生寻路优化
        /// </summary>
        static bool ActiveOptimizeGuestPathFind { get { return CustomSettings.OptimizeGuestPathFind; } set { CustomSettings.OptimizeGuestPathFind = value; } }
        /// <summary>
        /// 启用移除名称长度限制
        /// </summary>
        static bool ActiveNameLengthUnLimit { get { return CustomSettings.NameLengthUnLimit; } set { CustomSettings.NameLengthUnLimit = value; } }
        /// <summary>
        /// 启用显示单位移动路径
        /// </summary>
        static bool ActiveDrawWay
        {
            get { return CustomSettings.DrawWay; }
            set
            {
                DrawWay(value);

                CustomSettings.DrawWay = value;
            }
        }
        /// <summary>
        /// 启用直供仓库
        /// </summary>
        static bool ActiveDirectSupplyStorage { get { return CustomSettings.DirectSupplyStorage; } set { CustomSettings.DirectSupplyStorage = value; } }

        #endregion

        #region 自定义存档设置

        static string SavePath { get { return $"{SaveLoadIO.PATH_LOCAL_ROOT}/{SaveLoadIO.PATH_SAVE_DATA_DIRECTORY}"; } }

        /// <summary>
        /// 当前自定义存档设置
        /// </summary>
        static GameSaveCustomSettings GameSaveCustomSet = new GameSaveCustomSettings();

        /// <summary>
        /// 仓库数量
        /// </summary>
        static int StorageCount { get { return BuildingMgr.List_Storage.Count; } }

        /// <summary>
        /// 读取存档
        /// LoadMgr.SO_LoadSettingC()
        /// </summary>
        /// <param name="__instance"></param>
        public static void SystemMgr_SystemPause(bool pause)
        {
            if (pause || !PlayDataMgr.Instance.IsLoadGame)
                return;

            D_Data data = PlayDataMgr.Instance.m_GameData;

            string path = $"{SavePath}/{data.DirectoryName}/{data.FileName}.set";

            bool result = BaseCommand.LoadObjectByJson(path, out GameSaveCustomSet);

            if (!result || GameSaveCustomSet == null)
                GameSaveCustomSet = new GameSaveCustomSettings();

            //加载共享仓库
            if (result && ActiveShareStorage)
                SetShareStorage(true, true);

            Debug.Log($"读取了 {path}，自定义存档设置读取 {result}");
        }

        /// <summary>
        /// 存档前
        /// 这里会莫名其妙的被调用，疑似是自动存档？
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_data"></param>
        public static void PlayDataMgr_Save()
        {
            //Debug.LogWarning("存档前，关闭共享仓库");

            SetShareStorage(false);
        }

        /// <summary>
        /// 存档后
        /// </summary>
        public static void PlayDataMgr_Save_Post()
        {
            //Debug.LogWarning("存档后，开启共享仓库");

            if (ActiveShareStorage)
                SetShareStorage(true, true);
        }

        /// <summary>
        /// 存档后
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_data"></param>
        public static void SaveLoadMgr_SaveAsync_Zip(D_Data dData)
        {
            if (GameSaveCustomSet == null)
                GameSaveCustomSet = new GameSaveCustomSettings();

            SaveCustomSavedSetting(dData);

            //if (ActiveShareStorage)
            //{
            //    Debug.LogWarning("存档后，开启共享仓库");

            //    SetShareStorage(true, true);
            //}
        }

        /// <summary>
        /// 保存自定义存档设置
        /// </summary>
        /// <param name="dData"></param>
        static void SaveCustomSavedSetting(D_Data dData)
        {
            string path = $"{SavePath}/{dData.DirectoryName}/{dData.FileName}.set";

            bool result = BaseCommand.SaveObjectToJson(path, GameSaveCustomSet);

            //Debug.LogWarning($"{SaveLoadIO.PATH_LOCAL_ROOT} , {SaveLoadIO.PATH_AUTOSAVE_DIRECTORY} , {SaveLoadIO.PATH_USER_DATA_DIRECTORY} , {SaveLoadIO.PATH_SAVE_DATA_DIRECTORY}");

            Debug.LogWarning($"保存了 {path}，自定义存档设置保存 {result}");
        }

        /// <summary>
        /// 启用共享仓库
        /// </summary>
        static bool ActiveShareStorage
        {
            get { return GameSaveCustomSet.shareStorage; }
            set
            {
                SetShareStorage(value);

                GameSaveCustomSet.shareStorage = value;
            }
        }

        static bool ShareStorageInLock = false;
        static bool ShareStorageClosed = true;
        /// <summary>
        /// 设置共享仓库
        /// </summary>
        /// <param name="value"></param>
        /// <param name="skip"></param>
        static bool SetShareStorage(bool value, bool skip = false)
        {
            if (ShareStorageInLock || (!skip && ActiveShareStorage == value))
                return false;

            if (StorageCount < 2)
                return true;

            ShareStorageInLock = true;

            //Debug.Log($"设置共享仓库为 {value}");

            //开启时
            if (value)
            {
                List<TileSt_Info> list = new List<TileSt_Info>();

                TileSt_Info stInfo;

                //迭代所有仓库建筑
                foreach (Building_Storage storage in BuildingMgr.List_Storage)
                {
                    //迭代所有物品
                    foreach (TileSt_Info info in storage.List_TileObj)
                    {
                        stInfo = list.Find(t => t.m_Type == info.m_Type);

                        //添加物品至汇总列表
                        if (stInfo == null)
                        {
                            stInfo = new TileSt_Info(info.m_Type, info.m_State, info.List_Reservation.Count);

                            list.Add(stInfo);
                        }
                        else
                        {
                            for (int i = 0; i < info.List_Reservation.Count; i++)
                            {
                                stInfo.List_Reservation.Add(0);
                            }
                        }
                    }

                    //Debug.Log($"仓库 {storage.m_CustomName} 共有 {storage.List_TileObj.Count} 种物品");
                }

                //将汇总的物品绑定至所有仓库
                BindAllStorageTileList(list);

                Debug.Log($"已汇总{list.Count}种物品");
            }
            //关闭时
            else
            {
                //清空非首个箱子的物品
                for (int i = 1; i < BuildingMgr.List_Storage.Count; i++)
                {
                    BuildingMgr.List_Storage[i].List_TileObj = new List<TileSt_Info>();

                    BuildingMgr.List_Storage[i].m_BuildInfoUI.InfoUpdate();
                }

                Debug.Log($"仓库1以外物品已清空");
            }

            UpdateStorageCapacity(value);

            ShareStorageInLock = false;

            ShareStorageClosed = !value;

            return true;
        }

        /// <summary>
        /// 启用自定义市民皮肤
        /// </summary>
        static bool ActiveCustomCitizenSkin
        {
            get { return GameSaveCustomSet.customCitizenSkin; }
            set
            {
                GameSaveCustomSet.customCitizenSkin = value;
            }
        }

        #endregion

        #region 开始游戏界面

        static StartSettingsUI startSetUI;
        /// <summary>
        /// 开始设置界面
        /// </summary>
        /// <param name="__instance"></param>
        public static void StartSettingsUI_Start(StartSettingsUI __instance)
        {
            startSetUI = __instance;

            //InitMainSceneUI();
        }

        //static bool mainIsInited = false;
        //static InputField seedInput;
        //static string MapSeed { get { return seedInput != null ? seedInput.text : ""; } }
        //public static void InitMainSceneUI()
        //{
        //    if (mainIsInited)
        //        return;

        //    seedInput = new GameObject().AddComponent<InputField>();

        //    RectTransform rect = seedInput.gameObject.AddComponent<RectTransform>();

        //    seedInput.gameObject.layer = LayerMask.NameToLayer("UI");

        //    seedInput.textComponent = new GameObject().AddComponent<Text>();

        //    seedInput.gameObject.AddComponent<CanvasRenderer>();

        //    seedInput.textComponent.transform.SetParent(seedInput.transform);

        //    seedInput.textComponent.transform.localPosition = Vector3.zero;

        //    seedInput.textComponent.transform.localScale = Vector3.one;

        //    seedInput.textComponent.gameObject.layer = LayerMask.NameToLayer("UI");

        //    seedInput.textComponent.rectTransform.sizeDelta = new Vector2(500, 30);

        //    seedInput.textComponent.fontSize = 30;

        //    seedInput.characterLimit = 8;

        //    seedInput.name = "Input_MapSeed";

        //    List<Transform> list = startSetUI.GetComponentsInChildren<Transform>().Where(t=>t.name.Equals("Obj_BasicInfo")).ToList();

        //    if (list.Count > 0)
        //    {
        //        seedInput.transform.SetParent(list[0]);

        //        seedInput.transform.localPosition = Vector3.zero;

        //        seedInput.transform.localScale = Vector3.one;

        //        rect.sizeDelta = new Vector2(500f, 30);
        //    }

        //    mainIsInited = true;

        //    Debug.Log($"开始界面初始化完毕，当前种子按钮 {seedInput.name} {seedInput.transform.position}");
        //}

        #endregion

        #region 界面

        /// <summary>
        /// 屏幕尺寸
        /// </summary>
        Vector2 ScreenSize { get { return new Vector2(Screen.width, Screen.height); } }
        /// <summary>
        /// 屏幕尺寸比例
        /// </summary>
        Vector2 ScreenSizeRatio { get { return ScreenSize / ScreenStandardSize; } }

        Rect windowRect = new Rect(Screen.width / 2f, Screen.height / 2f, ModSetUISize.x, ModSetUISize.y);
        void OnGUI()
        {
            windowRect = new Rect(windowRect.x, windowRect.y, ModSetUISize.x * ScreenSizeRatio.x, ModSetUISize.y * ScreenSizeRatio.y);

            string title = $"自定义设置 {version} - Q群：250722774";

            if (ActiveStartGameUI)
                windowRect = GUILayout.Window(0, windowRect, StartGameWindowFunc, title);
            else if (ActiveGameUI)
                windowRect = GUILayout.Window(1, windowRect, ModSetWindowFunc, title);

            if (OpenedCitizenInfo != null && !(OpenedCitizenInfo is GBot))
                GUILayout.Window(1, new Rect(Screen.width - SkinSetUISize.x * ScreenSizeRatio.x - 20f, Screen.height - SkinSetUISize.y * ScreenSizeRatio.y - 20f, SkinSetUISize.x * ScreenSizeRatio.x, SkinSetUISize.y * ScreenSizeRatio.y), SkinSetWindowFunc, "- 自定义皮肤设置 -");
        }

        //static int seedLength = 4;
        ///// <summary>
        ///// 地图种子
        ///// </summary>
        //static string mapSeed = "";
        //static string MapSeed
        //{
        //    get { return mapSeed.Equals("") ? GetRandomIntString(seedLength) : mapSeed; }
        //    set { mapSeed = value; }
        //}
        /// <summary>
        /// 开始游戏功能界面
        /// </summary>
        /// <param name="id"></param>
        void StartGameWindowFunc(int id)
        {
            GUILayout.BeginHorizontal();

            //mapSeed = GUILayout.TextField(mapSeed, seedLength);

            //if (GUILayout.Button("随机地图种子"))
            //    MapSeed = GetRandomIntString(seedLength);

            //if (GUILayout.Button($"粘贴地图种子：[{copySeed}]"))
            //    MapSeed = copySeed;

            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        ///// <summary>
        ///// 获得随机int文本
        ///// </summary>
        ///// <param name="length"></param>
        ///// <returns></returns>
        //static string GetRandomIntString(int length)
        //{
        //    string result = "";

        //    for (int i = 0; i < length; i++)
        //    {
        //        result += RandomInt(0, 10).ToString();
        //    }

        //    return result;
        //}

        static string audioName = "";
        static string copySeed = "";
        string MapSeed { get { return PlayDataMgr.StartSettingsData.SettlementData.Seed; } }
        /// <summary>
        /// MOD设置界面
        /// </summary>
        /// <param name="id"></param>
        void ModSetWindowFunc(int id)
        {
            string isCoppyed = MapSeed.Equals(copySeed) ? "已复制" : "点击复制";

            audioName = GUILayout.TextField(audioName);

            if (GUILayout.Button("播放"))
                AudioController.PlayUIOneShot(audioName, 1f, false, null);

            if (GUILayout.Button($"{isCoppyed}当前地图种子：[{MapSeed}]"))
            {
                copySeed = MapSeed;

                CopyTextToClipboard(copySeed);
            }

            GUILayout.Label("全局基础设置");

            if (GUILayout.Button("全部开启"))
            {
                ActiveAddTimeScale = true;
                ActiveTimeUpdateReducedSpeed = true;
                ActiveAddPopLimit = true;
                ActiveAddDroneCount = true;
                ActiveAddBotCount = true;
                ActiveOnlyGoodCharacteristic = true;
                ActiveHunterBeAttackGetHalfDamage = true;
                ActiveMoreExp = true;
                ActiveMoreCapacityByPower = true;
                ActiveAddStorageCapacity = true;
                ActiveDefaultSelectSameItem = true;
                ActiveContinuousTakeOutItems = true;
                ActiveActionNoDropGatheredItems = true;
                ActiveSheetMoreInfo = true;
                ActiveDropNoDamgeWithOurTeam = true;
                ActiveCustomSpecialUnit = true;
                ActiveShareHome = true;
                ActiveCustomNames = true;

                SaveCustomSettings();
            }

            if (GUILayout.Button("全部关闭"))
            {
                ActiveAddTimeScale = false;
                ActiveTimeUpdateReducedSpeed = false;
                ActiveAddPopLimit = false;
                ActiveAddDroneCount = false;
                ActiveAddBotCount = false;
                ActiveOnlyGoodCharacteristic = false;
                ActiveHunterBeAttackGetHalfDamage = false;
                ActiveMoreExp = false;
                ActiveMoreCapacityByPower = false;
                ActiveAddStorageCapacity = false;
                ActiveDefaultSelectSameItem = false;
                ActiveContinuousTakeOutItems = false;
                ActiveActionNoDropGatheredItems = false;
                ActiveSheetMoreInfo = false;
                ActiveDropNoDamgeWithOurTeam = false;
                ActiveCustomSpecialUnit = false;
                ActiveShareHome = false;
                ActiveCustomNames = false;

                SaveCustomSettings();
            }



            if (GUILayout.Button($"游戏速度加快：{GetButtonValue(ActiveAddTimeScale)}"))
            {
                ActiveAddTimeScale = !ActiveAddTimeScale;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"时间流速减慢：{GetButtonValue(ActiveTimeUpdateReducedSpeed)}"))
            {
                ActiveTimeUpdateReducedSpeed = !ActiveTimeUpdateReducedSpeed;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"人口限制翻倍：{GetButtonValue(ActiveAddPopLimit)}"))
            {
                ActiveAddPopLimit = !ActiveAddPopLimit;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"无人机数量翻倍：{GetButtonValue(ActiveAddDroneCount)}"))
            {
                ActiveAddDroneCount = !ActiveAddDroneCount;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"机器人数量翻倍：{GetButtonValue(ActiveAddBotCount)}"))
            {
                ActiveAddBotCount = !ActiveAddBotCount;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"移民全正面特性：{GetButtonValue(ActiveOnlyGoodCharacteristic)}"))
            {
                ActiveOnlyGoodCharacteristic = !ActiveOnlyGoodCharacteristic;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"猎手受伤减半：{GetButtonValue(ActiveHunterBeAttackGetHalfDamage)}"))
            {
                ActiveHunterBeAttackGetHalfDamage = !ActiveHunterBeAttackGetHalfDamage;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"经验获取提高50%：{GetButtonValue(ActiveMoreExp)}"))
            {
                ActiveMoreExp = !ActiveMoreExp;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"力量影响负重翻倍：{GetButtonValue(ActiveMoreCapacityByPower)}"))
            {
                ActiveMoreCapacityByPower = !ActiveMoreCapacityByPower;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"仓库容量翻倍：{GetButtonValue(ActiveAddStorageCapacity)}"))
            {
                ActiveAddStorageCapacity = !ActiveAddStorageCapacity;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"默认选中已拾起的物品：{GetButtonValue(ActiveDefaultSelectSameItem)}"))
            {
                ActiveDefaultSelectSameItem = !ActiveDefaultSelectSameItem;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"按住左键连续拾取物品：{GetButtonValue(ActiveContinuousTakeOutItems)}"))
            {
                ActiveContinuousTakeOutItems = !ActiveContinuousTakeOutItems;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"女王建造拆除不丢弃物品：{GetButtonValue(ActiveActionNoDropGatheredItems)}"))
            {
                ActiveActionNoDropGatheredItems = !ActiveActionNoDropGatheredItems;

                SaveCustomSettings();
            }

            //if (GUILayout.Button($"蓝图无需材料：{GetButtonValue(ActiveBluePrintNoNeedRes)}"))
            //{
            //    ActiveBluePrintNoNeedRes = !ActiveBluePrintNoNeedRes;

            //    SaveCustomSettings();
            //}

            if (GUILayout.Button($"贸易详细信息：{GetButtonValue(ActiveSheetMoreInfo)}"))
            {
                ActiveSheetMoreInfo = !ActiveSheetMoreInfo;

                SaveCustomSettings();

            }

            if (GUILayout.Button($"友方掉落无伤：{GetButtonValue(ActiveDropNoDamgeWithOurTeam)}"))
            {
                ActiveDropNoDamgeWithOurTeam = !ActiveDropNoDamgeWithOurTeam;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"特殊移民鼠鼠：{GetButtonValue(ActiveCustomSpecialUnit)}"))
            {
                ActiveCustomSpecialUnit = !ActiveCustomSpecialUnit;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"共享床位：{GetButtonValue(ActiveShareHome)}"))
            {
                ActiveShareHome = !ActiveShareHome;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"更多姓名：{GetButtonValue(ActiveCustomNames)}"))
            {
                ActiveCustomNames = !ActiveCustomNames;

                SaveCustomSettings();
            }

            GUILayout.Label("全局特殊设置");

            if (GUILayout.Button($"移民性别：{GetGenderValue(NewCitizenGenderLimit)}"))
            {
                NewCitizenGenderLimit = NewCitizenGenderLimit + 1 > 1 ? -1 : NewCitizenGenderLimit + 1;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"贸易通知：{GetButtonValue(ActiveTradeResultMessage)}"))
            {
                ActiveTradeResultMessage = !ActiveTradeResultMessage;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"乌托邦模式：{GetButtonValue(ActiveUtopiaMode)}"))
            {
                ActiveUtopiaMode = !ActiveUtopiaMode;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"和平模式：{GetButtonValue(ActiveSafeMode)}"))
            {
                ActiveSafeMode = !ActiveSafeMode;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"敌方死亡掉落：{GetButtonValue(ActiveEnemyDeadthDrop)}"))
            {
                ActiveEnemyDeadthDrop = !ActiveEnemyDeadthDrop;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"更多种植：{GetButtonValue(ActiveMorePlantingPlants)}"))
            {
                ActiveMorePlantingPlants = !ActiveMorePlantingPlants;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"无限人口：{GetButtonValue(ActivePoPUnLimit)}"))
            {
                ActivePoPUnLimit = !ActivePoPUnLimit;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"[测试]直供仓库：{GetButtonValue(ActiveDirectSupplyStorage)}"))
            {
                ActiveDirectSupplyStorage = !ActiveDirectSupplyStorage;

                SaveCustomSettings();
            }

            //if (GUILayout.Button($"[测试]餐桌优化：{GetButtonValue(ActiveOptimizeFoodTable)}"))
            //{
            //    ActiveOptimizeFoodTable = !ActiveOptimizeFoodTable;

            //    SaveCustomSettings();
            //}

            //if (GUILayout.Button($"AI优化：{GetButtonValue(ActiveOptimizeAI)}"))
            //{
            //    ActiveOptimizeAI = !ActiveOptimizeAI;

            //    SaveCustomSettings();
            //}

            if (GUILayout.Button($"[测试]物品存取寻路优化：{GetButtonValue(ActiveOptimizeBuyAndSellPathFind)}"))
            {
                ActiveOptimizeBuyAndSellPathFind = !ActiveOptimizeBuyAndSellPathFind;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"[测试]食物日用品寻路优化：{GetButtonValue(ActiveOptimizeFoodAndLifePathFind)}"))
            {
                ActiveOptimizeFoodAndLifePathFind = !ActiveOptimizeFoodAndLifePathFind;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"[测试]娱乐卫生寻路优化：{GetButtonValue(ActiveOptimizeGuestPathFind)}"))
            {
                ActiveOptimizeGuestPathFind = !ActiveOptimizeGuestPathFind;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"输入框字符无限长度：{GetButtonValue(ActiveNameLengthUnLimit)}"))
            {
                ActiveNameLengthUnLimit = !ActiveNameLengthUnLimit;

                SaveCustomSettings();
            }

            if (GUILayout.Button($"显示单位移动路径：{GetButtonValue(ActiveDrawWay)}"))
            {
                ActiveDrawWay = !ActiveDrawWay;

                SaveCustomSettings();
            }

            GUILayout.Label("存档独立设置");

            if (GUILayout.Button($"共用仓库：{GetButtonValue(ActiveShareStorage)}"))
            {
                ActiveShareStorage = !ActiveShareStorage;
                
                SaveCustomSavedSetting(GameData);
            }

            if (GUILayout.Button($"[测试]自定义市民皮肤：{GetButtonValue(ActiveCustomCitizenSkin)}"))
            {
                ActiveCustomCitizenSkin = !ActiveCustomCitizenSkin;
                
                SaveCustomSavedSetting(GameData);
            }

            GUILayout.Label("其他功能");

            if (GUILayout.Button($"导出数据（导出前不要打开相关数据文件）"))
            {
                OutPutGameDatas(true);
            }

            //if (GUILayout.Button($"贸易距离减半：{GetButtonValue(TestOnOff)}"))
            //{
            //    TestOnOff = !TestOnOff;

            //    Dictionary<Vector2Int, DiplomaticCountryData> dic = CountryDatas;

            //    foreach (KeyValuePair<Vector2Int, DiplomaticCountryData> keyValue in dic)
            //    {
            //        int dis = keyValue.Value.WorldDistance;

            //        int value = TestOnOff ? 1 : dis;

            //        GetPrivateValue(keyValue.Value).Field("_worldDistance").SetValue(value);

            //        Debug.Log($"城市 {keyValue.Value.Info.T_Name} 距离 {keyValue.Value.WorldDistance}/{dis}/{keyValue.Value.Info.Distance} 位置 {keyValue.Key}");
            //    }
            //}

            GUI.DragWindow();
        }

        /// <summary>
        /// 获得性别值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        string GetGenderValue(int value)
        {
            return value == -1 ? "<color=red>不限</color>" : value == 0 ? "<color=#6caeff>男性</color>" : "<color=#ffa8f8>女性</color>";
        }

        /// <summary>
        /// 获得按钮值
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        string GetButtonValue(bool result)
        {
            return result ? "<color=green>开</color>" : "<color=red>关</color>";
        }

        /// <summary>
        /// 皮肤设置界面
        /// </summary>
        /// <param name="id"></param>
        void SkinSetWindowFunc(int id)
        {
            GUIStyle categoryNameStyle = new GUIStyle(GUI.skin.label)
            {
                fixedWidth = 70,
                alignment = TextAnchor.MiddleRight
            };
            GUIStyle categoryValueStyle = new GUIStyle(GUI.skin.label)
            {
                fixedWidth = 240,
                alignment = TextAnchor.MiddleCenter
            };
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 60,
                alignment = TextAnchor.MiddleCenter
            };
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            GUILayout.Label($"[{OpenedCitizenInfo.m_ID}] {OpenedCitizenInfo.m_UnitName}", labelStyle);

            int i = 0;

            foreach (KeyValuePair<string, List<string>> keyValuePair in CitizenCustomCategorySkins)
            {
                SkinCategory(keyValuePair.Value, keyValuePair.Key, CitizenCustomSkinCategoryNames_CN[i], EditingCustomSkinIndex.TryGetValue(keyValuePair.Key, out int index) ? index : 0, categoryNameStyle, categoryValueStyle, buttonStyle);

                i++;
            }

            string text = OpenedSpcialCitizen ? "暂时无法修改特殊鼠鼠的皮肤" : "（预制）自定义后会忽略所有其他皮肤设置\n其他各部位自定义会优先于职业本身设置\n全部重置后即可恢复游戏默认的皮肤设置";

            GUILayout.Label($"<color=#b41717>{text}</color>", labelStyle);

            GUI.DragWindow();
        }

        /// <summary>
        /// 皮肤部位
        /// </summary>
        /// <param name="category"></param>
        /// <param name="skins"></param>
        void SkinCategory(List<string> skins, string category, string categoryCN, int index, GUIStyle categoryNameStyle, GUIStyle categoryValueStyle, GUIStyle buttonStyle)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label($"{categoryCN} : ", categoryNameStyle);

            string skin = skins[index].Equals("") ? "未指定" : skins[index];

            int length = skins.Count;

            GUILayout.Label($"{skin} ({index + 1} / {length})", categoryValueStyle);

            if (!OpenedSpcialCitizen && (category.Equals("Basic") || !OpenedCitizenHavePremanentSkin))
            {
                if (GUILayout.Button("重置", buttonStyle))
                {
                    SetEditingCustomSkin(skins, category, 0, length);
                }

                if (GUILayout.Button("随机", buttonStyle))
                {
                    SetEditingCustomSkin(skins, category, Random.Range(0, length), length);
                }

                if (GUILayout.Button("上一个", buttonStyle))
                {
                    SetEditingCustomSkin(skins, category, --index, length);
                }

                if (GUILayout.Button("下一个", buttonStyle))
                {
                    SetEditingCustomSkin(skins, category, ++index, length);
                }
            }
            else
                GUILayout.FlexibleSpace();
                //GUILayout.Label("", new GUIStyle() { fixedWidth = buttonStyle.fixedWidth * 4 + 6});

            GUILayout.EndHorizontal();
        }

        #endregion

        #region 操作监听

        /// <summary>
        /// 默认的游戏速度
        /// </summary>
        float DefaultTimeScale { get { return ActiveAddTimeScale ? accelerateValue : 1f; } }
        bool activeCheat;
        /// <summary>
        /// 启用作弊
        /// </summary>
        bool ActiveCheat
        {
            get { return activeCheat; }
            set
            {
                if (value && GameMenuIsActive)
                    return;

                activeCheat = value;

                CheatMgr.SetActive(value);
            }
        }
        bool fullMapMode;
        bool FullMapMode
        {
            get { return fullMapMode; }
            set
            {
                if (value && GameMenuIsActive)
                    return;

                if (FullMapMode == value)
                    return;

                fullMapMode = value;

                if (value)
                {
                    ActiveGameUI = false;

                    MapEditorMode = false;

                    CheatMgr.FOG_Off_Btn();
                }

                CameraMgr.ZoomSizeUpdate(value ? 30f : 6.2f);

                UnitMgr.m_Queen.m_CamChasing = !value;

                Time.timeScale = value ? 0.3f : DefaultTimeScale;
            }
        }
        bool mapEditorMode;
        /// <summary>
        /// 地图编辑模式
        /// </summary>
        bool MapEditorMode
        {
            get { return mapEditorMode; }
            set
            {
                if (value && GameMenuIsActive)
                    return;

                mapEditorMode = value;

                if (value)
                {
                    ActiveGameUI = false;

                    FullMapMode = false;
                }

                CameraMgr.ZoomSizeUpdate(value ? 20f : 6.2f);

                Time.timeScale = value ? 0.3f : DefaultTimeScale;

                PallateMode(value);
            }
        }
        float num;
        Vector2 move;
        void Update()
        {
            //StartGameInputListener();

            GameInputListener();

            //CustomQuickKey();

            //CustomListener();
        }

        #region 自定义快捷键

        //bool ConstructUI_Active { get { return ConstructUI.m_BuildUI.gameObject.activeSelf; } }
        //void CustomQuickKey()
        //{
        //    if (SystemMgr == null || TileMgr.m_GameLoading)
        //        return;

        //    if (TileMgr.IsSandBoxMode || SystemMgr.IsGamePause() || Time.timeScale == 0f || Queen == null || Queen.m_AlivePause || Queen.__instance.IsAirWalk())
        //        return;

        //    //1 - 基础建造
        //    if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.B))
        //    {
        //        if (!ConstructUI_Active)
        //            ConstructUI.OpenTab(ConstructUI.m_CategoryIndex > 0 ? ConstructUI.m_CategoryIndex : 1);
        //        else
        //            ConstructUI.ExitBtn();

        //        Debug.Log($"建造界面 {ConstructUI_Active}");
        //    }
        //}

        #endregion

        //static bool lis_OnlyIdle = true;
        //static void CustomListener()
        //{
        //    if (Input.GetKeyDown(KeyCode.Q) && Input.GetKey(KeyCode.LeftShift))
        //    {
        //        lis_OnlyIdle = !lis_OnlyIdle;

        //        Debug.LogWarning("只显示待机AI");
        //    }
        //}

        static bool inStartMenu;
        static bool activeStartGameUI;
        static bool ActiveStartGameUI
        {
            get { return inStartMenu && activeStartGameUI; }
            set
            {
                activeStartGameUI = value;
            }
        }
        ///// <summary>
        ///// 开始游戏场景输入监听
        ///// </summary>
        //void StartGameInputListener()
        //{
        //    if (!inStartMenu)
        //        return;

        //    if (Input.GetKeyDown(KeyCode.Escape))
        //        ActiveStartGameUI = false;

        //    if (Input.GetKeyDown(KeyCode.F1))
        //        ActiveStartGameUI = !ActiveStartGameUI;
        //}

        static bool activeGameUI;
        static bool ActiveGameUI
        {
            get { return activeGameUI; }
            set
            {
                if (activeGameUI == value)
                    return;

                if (value && GameMenuIsActive)
                    return;

                activeGameUI = value;

                SystemMgr.SystemPause(value);
            }
        }
        /// <summary>
        /// 游戏内菜单已打开
        /// </summary>
        static bool GameMenuIsActive
        {
            get
            {
                return GameMenuMgr.Instance != null && GameMenuMgr.Instance.IsActivate;
            }
        }

        float pressTime = 0f;
        /// <summary>
        /// 游戏内场景输入监听
        /// </summary>
        void GameInputListener()
        {
            if (!SystemMgr)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ActiveGameUI = false;

                FullMapMode = false;

                ActiveCheat = false;

                MapEditorMode = false;
            }

            //MOD设置窗口
            if (!FullMapMode && !MapEditorMode && (ActiveGameUI || !SystemMgr.IsGamePause()) && Input.GetKeyDown(KeyCode.F1))
                ActiveGameUI = !ActiveGameUI;
            //全图模式
            else if (Input.GetKeyDown(KeyCode.F2))
                FullMapMode = !FullMapMode;
            //作弊界面
            else if (Input.GetKeyDown(KeyCode.F3))
                ActiveCheat = !ActiveCheat;
            //地图编辑模式
            else if (Input.GetKeyDown(KeyCode.F4))
                MapEditorMode = !MapEditorMode;
            //立即到来移民
            else if (Input.GetKeyDown(KeyCode.F5))
            {
                if (GameMgr.Instance._CCUI != null && CountryGold >= ProsperityLevel * 100f)
                {
                    if (Input.GetKey(KeyCode.LeftShift))

                    CountryGold -= ProsperityLevel * 100f;

                    GameMgr.Instance._BuildingMgr.m_CC_Building.m_CitizenCaveCurTime = GameMgr.Instance._BuildingMgr.m_CC_Building.m_Info.EffectValue2_Num;

                    GameMgr.Instance._CCUI.MakeCitizenList();
                }
            }

            if (fullMapMode)
            {
                num = 0f;

                if (Input.GetKey(KeyCode.KeypadPlus))
                    num = Time.deltaTime * -50f;
                else if (Input.GetKey(KeyCode.KeypadMinus))
                    num = Time.deltaTime * 50f;

                move = Vector2.zero;

                if (Input.GetKey(KeyCode.Keypad8) || Input.GetKey(KeyCode.UpArrow))
                    move += new Vector2(0f, 1f);
                if (Input.GetKey(KeyCode.Keypad5) || Input.GetKey(KeyCode.DownArrow))
                    move += new Vector2(0f, -1f);
                if (Input.GetKey(KeyCode.Keypad4) || Input.GetKey(KeyCode.LeftArrow))
                    move += new Vector2(-1f, 0f);
                if (Input.GetKey(KeyCode.Keypad6) || Input.GetKey(KeyCode.RightArrow))
                    move += new Vector2(1f, 0f);

                if (num != 0f)
                    CameraMgr.ZoomSizeUpdate(CameraMgr.m_MainCam.orthographicSize + num);

                if (move != Vector2.zero)
                    CameraMgr.Tf_Update(new Vector2(CameraMgr.Tf_Camera.transform.position.x + move.x, CameraMgr.Tf_Camera.transform.position.y + move.y), true);
            }
        }

        void LateUpdate()
        {
            if (!SystemMgr)
                return;

            if (ActiveContinuousTakeOutItems && selectedOutQueenCheckBox != null && selectedOutQueenCheckBox.List_MiniInfo.Count > 0)
            {
                List<TileObject> list = selectedOutQueenCheckBox.m_SelectNum < selectedOutQueenCheckBox.List_MiniInfo.Count ? selectedOutQueenCheckBox.List_MiniInfo[selectedOutQueenCheckBox.m_SelectNum].List_TileObj : null;

                if (Input.GetKeyUp(KeyCode.Mouse0) || list == null || list.Count == 0 || list[0].m_Info.m_Type != selectedType || queen == null || queen.List_Gathering.Count >= queen.Get_HandCapacity())
                    selectedOutQueenCheckBox = null;
                else if (Input.GetKeyDown(KeyCode.Mouse0))
                    pressTime = 0f;
                else if (Input.GetKey(KeyCode.Mouse0))
                {
                    pressTime += Time.deltaTime;

                    if (pressTime > 0.2f)
                    {
                        selectedOutQueenCheckBox.GatherSelected();

                        pressTime = 0f;
                    }

                    //Debug.Log($"{selectedOutQueenCheckBox.m_SelectNum} {Time.deltaTime}/{pressTime}");
                }
            }
        }

        #endregion

        #region 跳转场景

        /// <summary>
        /// 加载跳转场景
        /// </summary>
        public static void LoadingSceneMgr_Start()
        {
            activeGameUI = false;

            activeStartGameUI = false;

            inStartMenu = false;
        }

        #endregion

        #region 地图加载

        /// <summary>
        /// 生成地图_清理所有未使用的内容（读取存档后）
        /// </summary>
        public static void TileMgr_All_NotUseListClear()
        {
            SetDesireCut();

            LoadCustomSkinSetting(GameData);

            LoadCitizenDatas();

            ActiveAddTimeScale = ActiveAddTimeScale;

            ActiveAddPopLimit = ActiveAddPopLimit;

            ActiveAddStorageCapacity = ActiveAddStorageCapacity;

            ActiveDrawWay = ActiveDrawWay;

            ActiveCustomSpecialUnit = ActiveCustomSpecialUnit;

            //ActiveUtopiaMode = ActiveUtopiaMode;

            ActivePoPUnLimit = ActivePoPUnLimit;

            //ActiveOptimizeAI = ActiveOptimizeAI;

            ActiveOptimizeAI = false;

            //Debug.Log($"已更新特殊单位的使用状态");
        }

        /// <summary>
        /// 加载市民数据
        /// </summary>
        static void LoadCitizenDatas()
        {
            SpecialCitizens.Clear();

            SpecialCitizenSkins.Clear();

            specialUnit = null;


            foreach (CustomCharInfo info in CustomCharInfo.Values)
                info.ClearUser();

            CitizenDesires.Clear();

            for (int i = 0; i < Citizens.Count; i++)
            {
                T_Citizen citizen = Citizens[i];

                CitizenDesires.Add(citizen, new CitizenDesireThreshold(citizen));

                //添加市民名称
                if (!citizen.m_UnitName.Trim().Equals("") && usedNames.IndexOf(citizen.m_UnitName) == -1)
                    usedNames.Add(citizen.m_UnitName);

                //特殊市民加载
                if (!TryGetSpecialUnit(citizen, out CustomSpecialUnit unit) || !AddSpecialCitizen(unit, citizen))
                {
                    Debug.LogWarning($"加载市民 {citizen.m_UnitName}");
                    continue;
                }

                Debug.LogWarning($"加载特殊市民 {unit.Name}");

                if (citizen.m_Power < unit.pow)
                    citizen.m_Power = unit.pow;

                if (citizen.m_Dex < unit.dex)
                    citizen.m_Dex = unit.dex;

                if (citizen.m_Int < unit.wit)
                    citizen.m_Int = unit.wit;

                citizen.NameUpdate();

                foreach (CharacterInfo info in citizen.List_CharInfoValue)
                {
                    UpdateCustomCharInfoUser(info.Name, citizen);
                }
            }
        }

        #endregion

        #region 数据加载

        /// <summary>
        /// 正面特性1
        /// </summary>
        static List<CharacterInfo> GoodCharDB_1 = new List<CharacterInfo>();
        /// <summary>
        /// 正面特性2
        /// </summary>
        static List<CharacterInfo> GoodCharDB_2 = new List<CharacterInfo>();
        /// <summary>
        /// 繁荣数据
        /// </summary>
        static List<ProspertiyInfo> ProsperityDB = new List<ProspertiyInfo>();
        /// <summary>
        /// 当前繁荣基线所属的数据管理器。
        /// </summary>
        static DB_Mgr ProsperityDBOwner = null;
        /// <summary>
        /// 防止繁荣原始表暂不可用时重复刷屏。
        /// </summary>
        static bool ProsperityBaselineFailureLogged = false;
        /// <summary>
        /// 仓库容量数据
        /// </summary>
        static Dictionary<BuildingName, int> StorageCapacityDatas = new Dictionary<BuildingName, int>();
        /// <summary>
        /// 仓库数据
        /// </summary>
        static Dictionary<BuildingName, BuildInfo> StorageDB = new Dictionary<BuildingName, BuildInfo>();
        /// <summary>
        /// 物品原始价格数据
        /// </summary>
        static Dictionary<TileType, int> TileOriginePriceDatas = new Dictionary<TileType, int>();
        /// <summary>
        /// 敌人掉落数据
        /// </summary>
        static Dictionary<EnemyType, CustomEnemyDrop> EnemyDropDatas = new Dictionary<EnemyType, CustomEnemyDrop>();

        /// <summary>
        /// 数据初始化
        /// </summary>
        /// <param name="__instance"></param>
        public static void DB_Mgr_Awake(DB_Mgr __instance)
        {
            OutPutGameDatas();

            LoadEnemyDropDatas();

            LoadGoodCharDB();

            LoadProsperityDB(__instance);

            LoadStorageDB();

            LoadTileOriginePriceDatas();

            LoadCustomSettings();

            if (ActiveMorePlantingPlants)
                SetMorePlantingDB(true);

            DirectSupplyClear();

            //Debug.Log("数据初始化");
        }

        /// <summary>
        /// 物品数据初始化
        /// </summary>
        /// <param name="__instance"></param>
        public static void DB_Mgr_Res_DB_Setting(DB_Mgr __instance)
        {
            LoadDefaultTileInfo();
        }

        /// <summary>
        /// 建筑数据初始化
        /// </summary>
        /// <param name="__instance"></param>
        public static void DB_Mgr_Build_DB_Setting(DB_Mgr __instance)
        {
            LoadDefaultBuildInfo();

            LoadDefaultBotCount();

            //加载乌托邦设置
            ActiveUtopiaMode = ActiveUtopiaMode;

            //加载机器人数量设置
            ActiveAddBotCount = ActiveAddBotCount;
        }

        /// <summary>
        /// Spine皮肤管理器
        /// </summary>
        public static void SpineDresserMgr_Awake()
        {
            LoadCitizenCustomSkins();
        }

        /// <summary>
        /// 加载市民自定义皮肤
        /// </summary>
        static void LoadCitizenCustomSkins()
        {
            if (CitizenCustomCategorySkins.Count > 0)
                return;

            EditingCustomSkinIndex.Clear();

            foreach (string name in CitizenCustomSkinCategoryNames)
            {
                List<string> list = new List<string>() { "" };

                CitizenCustomCategorySkins[name] = list;

                EditingCustomSkinIndex[name] = 0;
            }

            //获取所有皮肤
            SkeletonData skeletonData = GetPrivateValue<SkeletonData>(SpineDresserMgr.Instance, "skeletonData");

            ExposedList<Skin> skins = skeletonData.Skins;

            foreach (Skin skin in skins)
            {
                string[] strs = skin.Name.Split('/');

                string category = strs.Length > 1 ? strs[0] : "Base";

                string name = strs[strs.Length > 1 ? 1 : 0];

                //跳过多余的皮肤
                if (!CitizenCustomCategorySkins.TryGetValue(category, out List<string> names))
                    continue;

                names.Add(name);
            }

            Debug.LogWarning($"共加载 {skins.Count} 皮肤");

            foreach (KeyValuePair<string, List<string>> keyValuePair in CitizenCustomCategorySkins)
            {
                Debug.LogWarning($"当前部位 {keyValuePair.Key} 共有 {keyValuePair.Value.Count} 皮肤");
            }
        }

        /// <summary>
        /// 加载敌人掉落数据
        /// </summary>
        static void LoadEnemyDropDatas()
        {
            //CustomEnemyDrop eDrop = new CustomEnemyDrop();

            //TileDrop drop = new TileDrop();

            //drop.name = TileType.Bone;

            //eDrop.dropList.Add(drop);

            //eDrop.dropList.Add(drop);

            //Debug.Log($"存储掉落 {BaseCommand.SaveCsvData($"{CustomDataPath}CustomEnemyDropDatas.csv", new List<CustomEnemyDrop>() { eDrop }, new List<List<string>>() { new List<string>() { "Name", "T_Name", "DropList" } })}");

            if (!BaseCommand.LoadCsvData($"{CustomDataPath}CustomEnemyDrop.csv", out List<CustomEnemyDrop> list))
                list = new List<CustomEnemyDrop>();

            EnemyDropDatas.Clear();

            foreach (CustomEnemyDrop drop1 in list)
            {
                EnemyDropDatas.Add(drop1.name, drop1);
            }
        }

        /// <summary>
        /// 加载物品原始价格数据
        /// </summary>
        static void LoadTileOriginePriceDatas()
        {
            TileOriginePriceDatas.Clear();

            foreach (TileInfo info in DBMgr.Dic_TileDB.Values)
            {
                TileOriginePriceDatas.Add(info.m_TileType, info.OriginPrice);
            }
        }

        /// <summary>
        /// 加载繁荣数据
        /// </summary>
        static bool LoadProsperityDB(DB_Mgr manager)
        {
            ProsperityDB.Clear();
            ProsperityDBOwner = null;

            if (manager == null || manager.m_Prosperity_DB1 == null ||
                manager.m_Prosperity_DB1.sheets == null || manager.m_Prosperity_DB1.sheets.Count == 0 ||
                manager.m_Prosperity_DB1.sheets[0] == null || manager.m_Prosperity_DB1.sheets[0].list == null)
            {
                LogProsperityBaselineFailure("原始繁荣数据尚未加载");
                return false;
            }

            foreach (Prosperity_DB1.Param db in manager.m_Prosperity_DB1.sheets[0].list)
            {
                if (db.Level == 0)
                    continue;

                ProsperityDB.Add(new ProspertiyInfo(db));
            }

            ProsperityDB.Sort((ProspertiyInfo x1, ProspertiyInfo x2) => x1.Level.CompareTo(x2.Level));

            if (ProsperityDB.Count == 0)
            {
                LogProsperityBaselineFailure("原始繁荣数据没有有效等级");
                return false;
            }

            ProsperityDBOwner = manager;
            ProsperityBaselineFailureLogged = false;
            return true;
        }

        /// <summary>
        /// 确保秦律始终使用当前数据库实例的原始繁荣基线。
        /// </summary>
        static bool EnsureProsperityBaseline()
        {
            DB_Mgr manager;
            try
            {
                manager = DBMgr;
            }
            catch (Exception error)
            {
                LogProsperityBaselineFailure($"数据管理器不可用：{error.GetType().Name}");
                return false;
            }

            if (manager == null || manager.List_ProsperityDB == null)
            {
                LogProsperityBaselineFailure("运行时繁荣数据不可用");
                return false;
            }

            List<int> liveLevels = manager.List_ProsperityDB.Select(info => info.Level).ToList();
            List<int> baselineLevels = ProsperityDB.Select(info => info.Level).ToList();
            if (ReferenceEquals(ProsperityDBOwner, manager) &&
                ProsperityBaselinePolicy.Matches(liveLevels, baselineLevels))
            {
                return true;
            }

            return LoadProsperityDB(manager) &&
                ProsperityBaselinePolicy.Matches(
                    manager.List_ProsperityDB.Select(info => info.Level).ToList(),
                    ProsperityDB.Select(info => info.Level).ToList());
        }

        static void LogProsperityBaselineFailure(string reason)
        {
            if (ProsperityBaselineFailureLogged)
                return;

            ProsperityBaselineFailureLogged = true;
            Debug.LogError($"繁荣等级基线初始化失败：{reason}；本次跳过秦律更新");
        }

        /// <summary>
        /// 加载仓库数据
        /// </summary>
        static void LoadStorageDB()
        {
            StorageCapacityDatas.Clear();

            StorageDB.Clear();

            foreach (KeyValuePair<BuildingName, BuildInfo> keyValue in DBMgr.Dic_BuildDB)
            {
                if (keyValue.Value.Ability != BuildAbility.Store)
                    continue;

                StorageDB.Add(keyValue.Key, keyValue.Value);

                StorageCapacityDatas.Add(keyValue.Key, (int)keyValue.Value.EffectValue1_Num);
            }
        }

        /// <summary>
        /// 加载正面特性
        /// </summary>
        static void LoadGoodCharDB()
        {
            GoodCharDB_1 = DBMgr.m_CharacterDB.List_Char1_DB.Where(t => GoodCharacteristicNames.IndexOf(t.Name) != -1).ToList();

            GoodCharDB_2 = DBMgr.m_CharacterDB.List_Char2_DB.Where(t => GoodCharacteristicNames.IndexOf(t.Name) != -1).ToList();
        }

        /// <summary>
        /// 导出游戏数据
        /// </summary>
        static void OutPutGameDatas(bool force = false)
        {
            if (!force && !OutPutDatas)
                return;

            //物品
            OutPutCSVDatas("Dic_TileDB", DBMgr.Dic_TileDB.Values.ToList());
            //建筑
            OutPutCSVDatas("Dic_BuildDB", DBMgr.Dic_BuildDB.Values.ToList());
            //锁定的建筑
            OutPutCSVDatas("Dic_BlockBuildDB", DBMgr.Dic_BlockBuildDB.Values.ToList());
            //铁路
            OutPutCSVDatas("Dic_RailDB", DBMgr.Dic_RailDB.Values.ToList());
            //升降轨道
            OutPutCSVDatas("Dic_LiftRailDB", DBMgr.Dic_LiftRailDB.Values.ToList());
            //植物
            OutPutCSVDatas("List_PlantDB", DBMgr.List_PlantDB);
            //敌人
            OutPutCSVDatas("List_EnemyDB", DBMgr.m_EnemyDB._list);
            //动物
            OutPutCSVDatas("List_AnimalDB", DBMgr.m_AnimalDB._list);
            //地图物品
            OutPutCSVDatas("List_MapObjDB", DBMgr.m_MapObjectDB._list);
            //国家
            //OutPutCSVDatas("List_CountryDB", DBMgr.List_CountryDB);
            //随机事件
            OutPutCSVDatas("List_RandEventDB", DBMgr.List_RandEventDB);
            //繁荣
            OutPutCSVDatas("List_ProsperityDB", DBMgr.List_ProsperityDB);
            //军事建筑
            OutPutCSVDatas("List_MilitaryDB", DBMgr.m_MilitaryDB._list);
            //物品
            OutPutCSVDatas("List_ItemDB", DBMgr.List_ItemDB);
            //武器
            OutPutCSVDatas("List_WeaponDB", DBMgr.List_WeaponDB);
            //衣服（防具）
            OutPutCSVDatas("List_ClothesDB", DBMgr.List_ClothesDB);
            //配件
            OutPutCSVDatas("List_AccessoryDB", DBMgr.List_AccessoryDB);
            //女王角色
            OutPutCSVDatas("List_QueenCharacterDB", DBMgr.List_QueenCharacterDB);
            //食物类型
            OutPutCSVDatas("List_FoodType", DBMgr.List_FoodType);
            //生活用品类型
            OutPutCSVDatas("List_LifeType", DBMgr.List_LifeType);
            //技能
            OutPutCSVDatas("List_Tech_DB", DBMgr.List_Tech_DB);
            //特征1
            OutPutCSVDatas("List_Char1_DB", DBMgr.m_CharacterDB.List_Char1_DB);
            //特征2
            OutPutCSVDatas("List_Char2_DB", DBMgr.m_CharacterDB.List_Char2_DB);
            //机器鼠特征1
            OutPutCSVDatas("List_RatronChar1_DB", DBMgr.List_RatronChar1_DB);
            //机器鼠特征2
            OutPutCSVDatas("List_RatronChar1_DB", DBMgr.List_RatronChar2_DB);
            //
            OutPutCSVDatas("List_RatronDB", DBMgr.List_RatronDB);
            //机器鼠类型
            OutPutCSVDatas("List_HeadDB", DBMgr.List_HeadDB);
            //能力
            OutPutCSVDatas("List_Ability_DB", DBMgr.m_AbilityDB._list);
            //
            OutPutCSVDatas("List_BodyDB", DBMgr.List_BodyDB);
            //黑暗等级
            OutPutCSVDatas("List_DarkLevelNode", DBMgr.List_DarkLevelNode);
            //光明等级
            OutPutCSVDatas("List_PartsDB", DBMgr.List_SunLevelNode);
            //
            OutPutCSVDatas("List_EarthratDB", DBMgr.List_EarthratDB);
            //
            OutPutCSVDatas("List_PartsDB", DBMgr.List_PartsDB);
            //
            OutPutCSVDatas("List_Wave_DB", DBMgr.List_Wave_DB);
            //
            OutPutCSVDatas("List_ScientistTech_DB", DBMgr.List_ScientistTech_DB);
            //
            OutPutCSVDatas("List_MagicianTech_DB", DBMgr.List_MagicianTech_DB);
            //
            OutPutCSVDatas("List_DmBlock_Bd", DBMgr.List_DmBlock_Bd);

            LocalizationManager.InitializeIfNeeded();
            //能力
            OutPutCSVDatas("LanguageData", LocalizationManager.Sources[0].mLanguages);

            //皮肤配置
            OutPutJsonData("SkinBunlde", SpineDresserMgr.Instance.Bundle);

            Debug.LogWarning("皮肤文件已导出");

            #region 手动分析数据

            //string value = "下标,名称,中文名称,等级,出口,进口\n";

            //foreach (CountryInfo countryInfo in DBMgr.list_)
            //{
            //    value += $"{countryInfo.Index},{countryInfo.Name},{countryInfo.T_Name},{countryInfo.ProsLevel},{GetDicContent(countryInfo.Dic_Export)},{GetDicContent(countryInfo.Dic_Import)}\n";
            //}

            //BaseCommand.SaveFile($"{DataPath}城市数据.csv", DataPath, value);

            //BaseCommand.SaveCsvData($"{DataPath}默认时间表.csv", DataPath, SystemMgr.m_WorkTable_Basic.ToList());

            #endregion

        }

        /// <summary>
        /// 获得字典内容
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        static string GetDicContent(Dictionary<int, List<TileType>> dic)
        {
            string text = "", keyText = "";

            foreach (KeyValuePair<int, List<TileType>> keyValue in dic)
            {
                if (!text.Equals(""))
                    text += "    ";

                keyText = $"[繁荣{keyValue.Key}]";

                text += $"{keyText}{string.Join($"    {keyText}", keyValue.Value.Select(t => DBMgr.Dic_TileDB[t].T_Name).ToArray())}";
            }

            return $"\"{text}\"";
        }

        /// <summary>
        /// 导出Json数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        static void OutPutJsonData(string fileName, object data)
        {
            string result = BaseCommand.SaveObjectToJson($"{DataPath}{fileName}.json", data) ? "成功" : "失败";

            Debug.Log($"{fileName}导出{result}");
        }

        /// <summary>
        /// 导出CSV数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <param name="list"></param>
        static void OutPutCSVDatas<T>(string fileName, List<T> list)
        {
            string result = BaseCommand.SaveCsvData($"{DataPath}{fileName}.csv", DataPath, list) ? "成功" : "失败";

            Debug.Log($"{fileName}导出{result}，共 {list.Count} 数据");
        }

        /// <summary>
        /// 设置更多种植数据
        /// </summary>
        /// <param name="value"></param>
        static void SetMorePlantingDB(bool value)
        {
            //荧光草 -> 发光体
            UpdatePlantingDatas(BuildingName.Minigarden, TileType.LightGrassPlant, TileType.Luminator, value);
            //仙人掌 -> 仙人花
            UpdatePlantingDatas(BuildingName.Minigarden, TileType.CactusPlant, TileType.CactusFlower, value);
            //纸莎草 -> 纸莎草
            UpdatePlantingDatas(BuildingName.Minigarden, TileType.PapyrusPlant, TileType.Papyrus, value);

            //欢乐花 -> 欢乐花
            //UpdatePlantingDatas(BuildingName.Treegarden, TileType.FunnyFlowerPlant, TileType.FunnyFlower, value);
            //珊瑚树 -> 珊瑚
            UpdatePlantingDatas(BuildingName.Treegarden, TileType.CoralTreePlant, TileType.Coral, value);
        }

        /// <summary>
        /// 更新种植数据
        /// </summary>
        /// <param name="buildName"></param>
        /// <param name="plantType"></param>
        /// <param name="recipeType"></param>
        /// <param name="add"></param>
        static void UpdatePlantingDatas(BuildingName buildName, TileType plantType, TileType recipeType, bool add = true)
        {
            if (!DBMgr.Dic_BuildDB.TryGetValue(buildName, out BuildInfo build))
            {
                Debug.LogWarning($"获取 {buildName} 失败！");

                return;
            }

            if (add)
            {
                build.List_Effect3.Add(plantType);

                build.List_Effect3_Num.Add(0);

                PlantInfo plant = DBMgr.List_PlantDB.Find(t => t.NameType == plantType);

                plant.List_Recipe.Add(recipeType);

                plant.List_Recipe_Num.Add(1);

                //plant.List_Seed.Add(recipeType);

                //plant.List_Seed_Num.Add(1);
            }
            else
            {
                int index = build.List_Effect3.FindIndex(t => t == plantType);

                if (index != -1)
                {
                    build.List_Effect3.RemoveAt(index);

                    build.List_Effect3_Num.RemoveAt(index);
                }

                PlantInfo plant = DBMgr.List_PlantDB.Find(t => t.NameType == plantType);

                index = plant.List_Recipe.FindIndex(t => t == plantType);

                if (index != -1)
                {
                    plant.List_Recipe.RemoveAt(index);

                    plant.List_Recipe_Num.RemoveAt(index);

                    //plant.List_Seed.RemoveAt(index);

                    //plant.List_Seed_Num.RemoveAt(index);
                }
            }

            Debug.LogWarning($"更新 {buildName} 种植配方 {plantType} 产出 {recipeType}, {add}");
        }

        #endregion

        #region 界面加载

        /// <summary>
        /// 加载政策界面（加载建筑后）
        /// </summary>
        /// <param name="__instance"></param>
        public static void PolicyUI_LoadSetting()
        {
            if (ActiveShareStorage && StorageCount >= 2)
                BindAllStorageTileList(BuildingMgr.List_Storage[1].List_TileObj);
        }

        #endregion

        #region 游戏速度加快

        public static bool SystemMgr_SetTimeScale(ref float value)
        {
            if (!ActiveAddTimeScale)
                return true;

            if (value.Equals(1f))
                value = accelerateValue;

            //SystemMgr.SetTimeScale(__value);

            return true;
        }

        #endregion

        #region 时间流逝减慢

        /// <summary>
        /// 时间流逝的速度
        /// </summary>
        static float TimeUpdateSpeed = 0.5f;
        public unsafe static void SystemMgr_Update()
        {
            if (!ActiveTimeUpdateReducedSpeed || GameMgr.Instance == null)
                return;

            SystemMgr sysMgr = GameMgr.Instance._SysMgr;

            if (sysMgr == null || GameMgr.Instance._TileMgr == null || GameMgr.Instance._TileMgr.m_GameLoading || sysMgr.IsGamePause())
                return;

            float rebackValue = -Time.deltaTime * (sysMgr.IsSpaceOut ? 0.05f : 1f);

            sysMgr.m_Second += rebackValue * (1 - TimeUpdateSpeed);
        }

        #endregion

        #region 自定义角色、特性

        /// <summary>
        /// 自定义角色特性
        /// </summary>
        static Dictionary<string, CharacterInfo> CustomCharacterInfoDatas = new Dictionary<string, CharacterInfo>();
        /// <summary>
        /// 自定义特殊单位
        /// </summary>
        static Dictionary<string, CustomSpecialUnit> CustomSpecialUnitDatas = new Dictionary<string, CustomSpecialUnit>();
        /// <summary>
        /// 自定义特殊单位随机组
        /// </summary>
        static Dictionary<int, List<CustomSpecialUnit>> CustomSpecialUnitRandomGroup = new Dictionary<int, List<CustomSpecialUnit>>();
        /// <summary>
        /// 特殊市民
        /// </summary>
        static Dictionary<string, T_Citizen> SpecialCitizens = new Dictionary<string, T_Citizen>();

        /// <summary>
        /// 自定义特性
        /// </summary>
        static Dictionary<string, CustomCharInfo> CustomCharInfo = new Dictionary<string, CustomCharInfo>
        {
            { "NaiNai_Wisdom", new CustomCharInfo(C_Buff.Exp_Up) },
            { "NaiNai_Benevolence", new CustomCharInfo(C_Buff.HappyUp) },
            { "LB_Sad", new CustomCharInfo(C_Buff.HappyDown) },
            { "LB_Hope", new CustomCharInfo(C_Buff.HappyUp) },
            { "SY_KCL", new CustomCharInfo(C_Buff.HappyUp) },
            { "SY_QL", new CustomCharInfo(C_Buff.None) },
            { "YF_YJQ", new CustomCharInfo(C_Buff.None) },
            { "YF_YJJ", new CustomCharInfo(C_Buff.PowerUp) },
            { "HT_SYZS", new CustomCharInfo(C_Buff.None) },
            { "HT_WQX", new CustomCharInfo(C_Buff.HappyUp) },
            { "WH_NC", new CustomCharInfo(C_Buff.None) },
            { "WH_SZ", new CustomCharInfo(C_Buff.None) },
            { "BG_NYQY", new CustomCharInfo(C_Buff.None) },
            { "BG_SS", new CustomCharInfo(C_Buff.None) },
            { "LLJ_KYSS", new CustomCharInfo(C_Buff.None) },
            { "LLJ_LY", new CustomCharInfo(C_Buff.HappyUp) },
            { "DZ_MGJZ", new CustomCharInfo(C_Buff.None) },
            { "DZ_MGZL", new CustomCharInfo(C_Buff.HappyUp) },
            { "ZY_QTSPQ", new CustomCharInfo(C_Buff.None) },
            { "ZY_LD", new CustomCharInfo(C_Buff.DexUp) },
            { "PKQ_SWFT", new CustomCharInfo(C_Buff.None) },
            { "PKQ_DQCD", new CustomCharInfo(C_Buff.None) },
            { "AMJ7_LZDW", new CustomCharInfo(C_Buff.None) },
            { "AMJ7_LZJX", new CustomCharInfo(C_Buff.None) },
        };

        ///// <summary>
        ///// 初始特性1列表
        ///// </summary>
        //static List<CharacterInfo> DefaultChar1List = new List<CharacterInfo>();
        ///// <summary>
        ///// 初始特性2列表
        ///// </summary>
        //static List<CharacterInfo> DefaultChar2List = new List<CharacterInfo>();
        /// <summary>
        /// 所有特性
        /// </summary>
        static List<CharacterInfo> AllCharInfos = new List<CharacterInfo>();
        /// <summary>
        /// 初始特性1数量
        /// </summary>
        static int DefaultChar1Count = 0;
        /// <summary>
        /// 初始特性2数量
        /// </summary>
        static int DefaultChar2Count = 0;

        /// <summary>
        /// 加载特性设置
        /// </summary>
        /// <param name="__instance"></param>
        public static void DB_Mgr_Character_DB_Setting(DB_Mgr __instance)
        {
            ActiveShareStorage = false;

            AllCharInfos.Clear();

            AllCharInfos.AddRange(__instance.m_CharacterDB.List_Char1_DB);

            AllCharInfos.AddRange(__instance.m_CharacterDB.List_Char2_DB);

            LoadDefaultCharList();

            LoadCustomDatas();

            preValueDic.Clear();

            LoadProsperityDB(__instance);

            //Debug.Log("加载特性设置");
        }

        /// <summary>
        /// 加载初始特性列表
        /// </summary>
        static void LoadDefaultCharList()
        {
            if (DefaultChar1Count > 0)
                return;

            DefaultChar1Count = DBMgr.m_CharacterDB.List_Char1_DB.Count;

            DefaultChar2Count = DBMgr.m_CharacterDB.List_Char2_DB.Count;

            Debug.Log($"已加载 {DefaultChar1Count} 特性1 {DefaultChar2Count} 特性2");
        }

        /// <summary>
        /// 加载自定义数据
        /// </summary>
        static void LoadCustomDatas()
        {
            SpecialDataCatalog.Load(
                Path.Combine(CustomDataPath, "CustomSpecialUnit.csv"),
                Path.Combine(CustomDataPath, "CustomCharInfo.csv"),
                Path.Combine(CustomDataPath, "Icon"));

            //加载自定义特性
            BaseCommand.LoadCsvData($"{CustomDataPath}CustomCharInfo.csv", out List<CharacterInfo> customInfos);

            //加载特殊单位
            BaseCommand.LoadCsvData($"{CustomDataPath}CustomSpecialUnit.csv", out List<CustomSpecialUnit> customUnits);

            if (customInfos == null || customUnits == null)
                throw new InvalidDataException("特殊鼠鼠 CSV 通过预检，但旧版兼容解析器无法读取。未修改游戏特性数据库。");

            CustomCharacterInfoDatas.Clear();

            CustomSpecialUnitDatas.Clear();

            CustomSpecialUnitRandomGroup.Clear();

            Debug.LogWarning($"共加载 {customInfos.Count} 自定义特性，{customUnits.Count} 自定义单位");

            //添加自定义特性
            foreach (CharacterInfo customInfo in customInfos)
            {
                CustomCharacterInfoDatas.Add(customInfo.Name, customInfo);
            }

            foreach (CustomSpecialUnit customUnit in customUnits)
            {
                if (customUnit.lockStatus == CustomSpecialUnit.Lock_Status.Lock)
                    continue;

                if (!CustomCharacterInfoDatas.TryGetValue(customUnit.char1, out CharacterInfo char1))
                {
                    Debug.Log($"自定义单位 {customUnit.name} 特性 {customUnit.char1} 配置错误！");

                    continue;
                }

                if (!CustomCharacterInfoDatas.TryGetValue(customUnit.char2, out CharacterInfo char2))
                {
                    Debug.Log($"自定义单位 {customUnit.name} 特性 {customUnit.char2} 配置错误！");

                    continue;
                }

                customUnit.char_1 = char1;

                customUnit.char_2 = char2;

                customUnit.pdr_C = 0;

                customUnit.isUsed = false;

                CustomSpecialUnitDatas.Add(customUnit.name, customUnit);

                if (!CustomSpecialUnitRandomGroup.TryGetValue(customUnit.grade, out List<CustomSpecialUnit> list))
                {
                    list = new List<CustomSpecialUnit>()
                    {
                        customUnit
                    };

                    CustomSpecialUnitRandomGroup.Add(customUnit.grade, list);
                }
                else
                    list.Add(customUnit);

                RegisterCustomCharInfo(char1, customUnit.icon1);

                RegisterCustomCharInfo(char2, customUnit.icon2);

                Debug.LogWarning($"自定义单位 {customUnit.name} 注册完毕，特性1 {char1.T_Name} 特性2 {char2.T_Name}");
            }
        }

        /// <summary>
        /// 注册自定义特性
        /// </summary>
        /// <param name="info"></param>
        /// <param name="iconAddress"></param>
        static void RegisterCustomCharInfo(CharacterInfo info, string iconAddress = "")
        {
            //正常情况下已预制
            if (!TryGetCustomCharInfo(info.Name, out CustomCharInfo charInfo))
            {
                charInfo = new CustomCharInfo(C_Buff.None);

                CustomCharInfo.Add(info.Name, charInfo);
            }

            charInfo.name = info.Name;

            charInfo.iconAddress = iconAddress;

            charInfo.t_name = info.T_Name;

            charInfo.value1 = info.EffectValue_A;

            charInfo.value2 = info.EffectValue_B;

            charInfo.description = info.Description;

            UpdateCharDB(info);

            RegisterCustomInfoIcon(info);
        }

        /// <summary>
        /// 更新特性数据库
        /// List_Char1_DB 数据目前使用了ScriptableObject，每次读档后会丢失汉字数据？
        /// 原特性通过 DefaultChar1List、DefaultChar2List 在第一加载时保存
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        static void UpdateCharDB(CharacterInfo info)
        {
            List<CharacterInfo> charList = info.Category == 0 ? DBMgr.m_CharacterDB.List_Char1_DB : DBMgr.m_CharacterDB.List_Char2_DB;

            int lastIndex = charList[charList.Count - 1].Index;

            string infoName = info.Name;

            //获得列表中相同的特性
            List<CharacterInfo> list = charList.Where(t => t.Name.Equals(infoName)).ToList();

            //添加特性进数据表
            if (list.Count == 0)
            {
                //绑定下标为末尾+1
                info.Index = lastIndex + 1;

                lastIndex++;

                charList.Add(info);

                //Debug.Log($"已添加 [{info.Index}/{lastIndex}] {info.T_Name} 效果 {info.Description}");
            }
            else
            {
                info.Index = list[0].Index;

                list[0].T_Name = info.T_Name;

                list[0].Description = info.Description;

                //Debug.Log($"已更新 [{info.Index}/{lastIndex}] {list[0].T_Name} 效果 ({list[0].EffectValue_A}, {list[0].EffectValue_B}), {list[0].Description}");
            }

            //Debug.Log($"dbList({info.Category}) : {charList.Count}, all : {AllCharInfos.Count}, char1 : {DB_DefaultChar1Count}, char2 : {DB_DefaultChar2Count}");
        }

        /// <summary>
        /// 注册自定义特性图标
        /// </summary>
        /// <param name="list"></param>
        /// <param name="lastIndex"></param>
        /// <param name="info"></param>
        static void RegisterCustomInfoIcon(CharacterInfo info)
        {
            if (info == null || !TryGetCustomCharInfo(info.Name, out CustomCharInfo customInfo))
                throw new InvalidDataException("注册特殊能力图标时找不到特性数据。");

            string iconKey = CustomIconKeys.ForTrait(info.Name);
            string indexKey = CustomIconKeys.ForCharacterIndex(info.Index);

            customInfo.iconKey = iconKey;

            string spriteName = customInfo.iconAddress;

            if (string.IsNullOrWhiteSpace(spriteName))
                throw new InvalidDataException($"特殊能力 {info.Name} 的图标地址为空。");

            string iconPath = Path.Combine(CustomDataPath, "Icon", $"{spriteName}.png");

            Sprite sprite = BaseCommand.LoadSpriteFromTexture2D(BaseCommand.LoadTextureFromFile(iconPath));

            if (sprite == null)
                throw new InvalidDataException($"特殊能力 {info.Name} 图标加载失败：{iconPath}");

            CharacterInfo indexedInfo = DBMgr.GetCharacterInfo(info.Index);

            if (indexedInfo == null || !string.Equals(indexedInfo.Name, info.Name, StringComparison.Ordinal))
                throw new InvalidDataException(
                    $"特殊能力 {info.Name} 的图标索引 {info.Index} 被其他特性占用：{indexedInfo?.Name ?? "<null>"}");

            Dictionary<string, Sprite> sprites = DicSprits;

            if (sprites == null)
                throw new InvalidDataException($"特殊能力 {info.Name} 无法访问游戏图标资源表。");

            sprites[iconKey] = sprite;
            sprites[indexKey] = sprite;

            //Debug.LogWarning($"注册自定义特性 [{info.Index}]{info.T_Name} 图标 {iconKey}/{indexKey} - {sprite.name}({spriteName})");
        }

        /// <summary>
        /// 添加特殊市民
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="citizen"></param>
        static bool AddSpecialCitizen(CustomSpecialUnit unit, T_Citizen citizen)
        {
            if (unit == null)
            {
                Debug.LogError($"添加特殊市民 {citizen.m_UnitName} 错误");

                return false;
            }

            citizen.m_UnitName = unit.Name;

            if (SpecialCitizens.ContainsKey(unit.Name))
            {
                Debug.LogWarning($"特殊市民 {citizen.m_UnitName} 已存在！");

                return false;
            }

            SpecialCitizens.Add(citizen.m_UnitName, citizen);

            //标记单位已出现
            unit.isUsed = true;

            unit.pdr_C = 0;

            citizen.m_SkinInfo.m_Gender = citizen.m_Gender;

            RegisterCustomSkin(citizen.m_SkinInfo, unit, true);

            Debug.LogWarning($"获得特殊市民 {citizen.m_UnitName}，三维 {GetPDIValue(citizen, 2f)}，特性1 {unit.char_1.T_Name} : {unit.char_1.Description}，特性2 {unit.char_2.T_Name} : {unit.char_2.Description}");

            //Debug.LogWarning($"皮肤 {unit.face.Trim()}/{SpecialCitizenSkins[citizen.m_UnitName]["Face"]} {unit.bread.Trim()}/{SpecialCitizenSkins[citizen.m_UnitName]["Bread"]} {unit.dress.Trim()}/{SpecialCitizenSkins[citizen.m_UnitName]["Dress"]}");

            return true;
        }

        /// <summary>
        /// 市民是特殊单位
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        static bool CitizenIsSpecialUnit(string name, int _id, out T_Citizen citizen)
        {
            if (!SpecialCitizens.TryGetValue(name, out citizen))
                return false;

            return citizen.m_ID == _id;
        }

        /// <summary>
        /// 市民是特殊单位
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        static bool CitizenIsSpecialUnit(T_Citizen citizen, string name = "")
        {
            return SpecialCitizens.TryGetValue(citizen.m_UnitName, out _) && (name.Equals("") || citizen.m_UnitName.Contains(name));
        }

        /// <summary>
        /// 市民有特性
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="infoName"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        static bool CitizenHaveCharacterInfo(T_Citizen citizen, string infoName, out CharacterInfo info)
        {
            List<CharacterInfo> list = citizen.List_CharInfoValue.Where(t => t.Name.Equals(infoName)).ToList();

            info = list.Count > 0 ? list[0] : null;

            return info != null;
        }

        /// <summary>
        /// 更新自定义特性使用者
        /// </summary>
        /// <param name="name"></param>
        /// <param name="user"></param>
        static CustomCharInfo UpdateCustomCharInfoUser(string name, T_Citizen user)
        {
            if (!TryGetCustomCharInfo(name, out CustomCharInfo customInfo))
            {
                Debug.LogWarning($"{user.m_UnitName} 自定义特性 {name} 获取失败");

                return null;
            }

            customInfo.User = user;

            return customInfo;
        }

        /// <summary>
        /// 待使用的特殊单位
        /// </summary>
        static CustomSpecialUnit specialUnit;
        /// <summary>
        /// 生成移民列表
        /// </summary>
        public static void CitizenCaveUI_MakeCitizenList()
        {
            if (!ActiveCustomSpecialUnit)
                return;

            List<CustomSpecialUnit> units = CustomSpecialUnitDatas.Values.ToList();
            List<SpecialCandidateState> states = units.Select(unit =>
                new SpecialCandidateState(unit.name, unit.grade, unit.probability, unit.isUsed)
                {
                    ProbabilityBonus = unit.pdr_C
                }).ToList();

            SpecialCandidateState selected = SpecialSelectionEngine.Select(states, ProsperityLevel, RandomInt);

            for (int i = 0; i < units.Count; i++)
                units[i].pdr_C = states[i].ProbabilityBonus;

            specialUnit = selected == null
                ? null
                : units.First(unit => unit.name.Equals(selected.Name, StringComparison.Ordinal));

            if (specialUnit != null)
                Debug.LogWarning($"出现特殊单位 {specialUnit.name}，当前概率 {specialUnit.RealProbability}/10000");
        }

        /// <summary>
        /// 生成角色信息
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_grade_max"></param>
        /// <returns></returns>
        public static bool CCMake_Info(CCMake_Info __instance, int _grade_max, bool _religion_check = false)
        {
            if (ActiveCustomSpecialUnit && specialUnit != null)
            {
                __instance.List_CharInfo = new List<int>();

                __instance.m_Gender = specialUnit.gender;
                __instance.Name = specialUnit.Name;
                __instance.Power = specialUnit.pow;
                __instance.Dex = specialUnit.dex;
                __instance.Int = specialUnit.wit;
                __instance.CitizenGold = specialUnit.gold;
                __instance.m_Religion = Religion.None;
                __instance.MakeSkinInfo();
                __instance.List_CharInfo.Add(specialUnit.char_1.Index);
                __instance.List_CharInfo.Add(specialUnit.char_2.Index);

                RegisterCustomSkin(__instance.SkinInfo, specialUnit, false);

                Debug.LogWarning($"创建了自定义角色 {__instance.Name}，特性1 [{specialUnit.char_1.Index}]{specialUnit.char_1.T_Name} 特性2 [{specialUnit.char_2.Index}]{specialUnit.char_2.T_Name}");

                AudioController.PlayUIOneShot("SFX_UI_Popup_Casting", 1f, false, null);

                specialUnit = null;

                return false;
            }

            if (NewCitizenGenderLimit != -1)
            {
                __instance.m_Gender = NewCitizenGenderLimit == 0 ? Gender.Male : Gender.Female;
                __instance.Name = GameMgr.Instance._CCUI.GetRandomName(__instance.m_Gender);
                __instance.Power = RandomInt(0, _grade_max);
                __instance.Dex = RandomInt(0, _grade_max);
                __instance.Int = RandomInt(0, _grade_max);
                __instance.CitizenGold = RandomInt(200, 350 + _grade_max * 15);
                __instance.m_Religion = Religion.None;
                if (_religion_check)
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        if (GameMgr.Instance._BuildingMgr.List_Building.Exists((Building x) => x.m_Info.Name == BuildingName.DarkSanctum) && GameMgr.Instance._T_UnitMgr.GetReligionNum(Religion.Dark) == 0)
                        {
                            __instance.m_Religion = Religion.Dark;
                        }
                        else if (GameMgr.Instance._BuildingMgr.List_Building.Exists((Building x) => x.m_Info.Name == BuildingName.Temple) && GameMgr.Instance._T_UnitMgr.GetReligionNum(Religion.Sun) == 0)
                        {
                            __instance.m_Religion = Religion.Sun;
                        }
                    }
                    else if (GameMgr.Instance._BuildingMgr.List_Building.Exists((Building x) => x.m_Info.Name == BuildingName.Temple) && GameMgr.Instance._T_UnitMgr.GetReligionNum(Religion.Sun) == 0)
                    {
                        __instance.m_Religion = Religion.Sun;
                    }
                    else if (GameMgr.Instance._BuildingMgr.List_Building.Exists((Building x) => x.m_Info.Name == BuildingName.DarkSanctum) && GameMgr.Instance._T_UnitMgr.GetReligionNum(Religion.Dark) == 0)
                    {
                        __instance.m_Religion = Religion.Dark;
                    }
                    if (__instance.m_Religion == Religion.None)
                    {
                        if (GameMgr.Instance._BuildingMgr.List_Building.Exists((Building x) => x.m_Info.Name == BuildingName.MagicianTable))
                        {
                            if (Random.Range(0, 2) == 0)
                            {
                                if (Random.Range(0, 10) < GameMgr.Instance._SysMgr.m_CC_ShowReligionPer)
                                {
                                    __instance.m_Religion = Religion.Dark;
                                }
                            }
                            else if (Random.Range(0, 10) < GameMgr.Instance._SysMgr.m_CC_ShowReligionPer)
                            {
                                __instance.m_Religion = Religion.Sun;
                            }
                        }
                    }
                }
                __instance.MakeSkinInfo();
                MakeCharacterList(__instance);

                return false;
            }

            return true;
        }

        /// <summary>
        /// 更新所有已启用的特性效果
        /// </summary>
        static void UpdateAllUsedSpecialEffects()
        {
            UpdateAllSelfSpecialEffects();

            for (int i = 0; i < Citizens.Count; i++)
            {
                UpdateCitizenUsedSpecialEffects(Citizens[i]);
            }
        }

        /// <summary>
        /// 更新所有单体特性效果
        /// </summary>
        static void UpdateAllSelfSpecialEffects()
        {
            SY_QL_Effect();

            AMJ7_LZDW_Effect();

            AMJ7_LZJX_Effect();
        }

        /// <summary>
        /// 更新单个市民已启用的特性效果
        /// </summary>
        static void UpdateCitizenUsedSpecialEffects(T_Citizen citizen)
        {
            #region 自身效果

            //联邦的哀伤
            UpdateSelfSpecialState(citizen, "LB_Sad");
            //联邦的希望
            UpdateSelfSpecialState(citizen, "LB_Hope");

            //奈奈的智慧
            UpdateSelfSpecialState(citizen, "NaiNai_Wisdom");

            //龙胆
            ZY_LD_Effect(citizen);

            #endregion

            #region 群体效果

            //奈奈的关爱
            UpdateSpecialState("NaiNai_Benevolence", NNBen_Value, citizen);

            //垦草令
            UpdateSpecialState("SY_KCL", KCL_Value, citizen);

            //岳家军
            YF_YJJ_Effect(citizen);

            //五禽戏
            HT_WQX_Effect(citizen);

            //梨园
            LLJ_LY_Effect(citizen);

            #endregion
        }

        public static void GBot_MakeCitizen(GBot __instance, int _index)
        {
            if (!ActiveCustomSpecialUnit)
                return;

            AMJ7_LZJX_Effect(__instance);
        }

        /// <summary>
        /// 通过移民生成市民
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_info"></param>
        public static void T_Citizen_MakeCtizen_ByCC(T_Citizen __instance, CCMake_Info _info)
        {
            if (!ActiveCustomSpecialUnit)
                return;

            UpdateCitizenUsedSpecialEffects(__instance);

            //跳过非特殊移民
            if (!TryGetSpecialUnit(__instance, out CustomSpecialUnit unit))
                return;

            AddSpecialCitizen(unit, __instance);

            //重置同等级下的其他单位的概率增值
            if (CustomSpecialUnitRandomGroup.TryGetValue(unit.grade, out List<CustomSpecialUnit> groupList))
                groupList.ForEach(t => t.pdr_C = 0);

            //这里只针对群体效果
            foreach (int index in _info.List_CharInfo)
            {
                CharacterInfo info = DBMgr.GetCharacterInfo(index);

                if (info == null)
                {
                    Debug.Log($"{_info.Name} 特性 {index} 加载失败！");

                    continue;
                }

                CustomCharInfo customInfo = UpdateCustomCharInfoUser(info.Name, __instance);

                switch (info.Name)
                {
                    //奈奈的仁爱
                    case "NaiNai_Benevolence":

                        UpdateSpecialStateToAllCitizen(customInfo.c_Buff, customInfo.name, NNBen_Value);

                        break;

                    //联邦的哀伤
                    case "LB_Sad":
                    //联邦的希望
                    case "LB_Hope":

                        __instance.m_Buff.BuffRefSet(customInfo.c_Buff, customInfo.name, C_Buff_Category.None, info.EffectValue_A, -999, true);

                        break;

                    //秦律
                    case "SY_QL":

                        SY_QL_Effect();

                        break;

                    //岳家军
                    case "YF_YJJ":

                        for (int i = 0; i < Citizens.Count; i++)
                        {
                            YF_YJJ_Effect(Citizens[i], false);
                        }

                        break;

                    //五禽戏
                    case "HT_WQX":

                        for (int i = 0; i < Citizens.Count; i++)
                        {
                            HT_WQX_Effect(Citizens[i], false);
                        }

                        break;

                    //梨园
                    case "LLJ_LY":

                        LLJ_LY_Effect();

                        break;

                    //量子电网
                    case "AMJ7_LZDW":

                        AMJ7_LZDW_Effect();

                        break;

                    //量子机械
                    case "AMJ7_LZJX":

                        AMJ7_LZJX_Effect();

                        break;
                }
            }

            // 特性使用者已经全部登记，此时再应用一次自身与群体状态，
            // 避免新招募角色（例如奈奈酱的“奈奈的智慧”）必须读档后才生效。
            UpdateCitizenUsedSpecialEffects(__instance);
        }

        #region 特性效果部分
        /// <summary>
        /// 量子机械值
        /// </summary>
        static int LZJX_Value
        {
            get
            {
                return ActiveCustomSpecialUnit && TryGetCustomValue("奥米伽-7", "AMJ7_LZJX", out _, out T_Citizen citizen, out _, out _) ? GetPDIValue(citizen) : 0;
            }
        }
        /// <summary>
        /// 奥米伽三维
        /// </summary>
        static int AMJ7_PDI = 0;
        /// <summary>
        /// 量子机械
        /// </summary>
        static void AMJ7_LZJX_Effect(GBot bot = null)
        {
            string key = "AMJ7_LZJX";

            bool canUse = TryGetCustomValue("奥米伽-7", key, out _, out T_Citizen citizen, out float value1, out float value2);

            List<GBot> list = bot != null ? new List<GBot> { bot } : UnitMgr.List_GBot;

            AMJ7_PDI = canUse ? GetPDIValue(citizen) : 0;

            float value = canUse ? (value1 + AMJ7_PDI / value2) / 100f : 0f;

            float[] addValues = canUse ? new float[] { citizen.m_Power * value, citizen.m_Dex * value, citizen.m_Int * value } : null;

            //多状态放在Effect进行迭代
            for (int i = 0; i < list.Count; i++)
            {
                list[i].m_Buff.RefKill(key);

                if (!canUse)
                    continue;

                UpdateSpecialStateToUnit(list[i], C_Buff.PowerUp, key, Mathf.FloorToInt(addValues[0]));
                UpdateSpecialStateToUnit(list[i], C_Buff.DexUp, key, Mathf.FloorToInt(addValues[1]), false, false);
                UpdateSpecialStateToUnit(list[i], C_Buff.IntUp, key, Mathf.FloorToInt(addValues[2]), false, false);

                FillUpGbotPower(list[i]);
            }

            if (bot != null)
                Debug.LogWarning($"机械 {bot.m_UnitName} 与奥米伽-7连接！");
            else
                Debug.LogWarning($"共 {UnitMgr.List_GBot.Count} 机械与奥米伽-7连接！");
        }
        /// <summary>
        /// 补满机械鼠电力
        /// </summary>
        /// <param name="bot"></param>
        /// <returns></returns>
        static bool FillUpGbotPower(GBot bot)
        {
            if (bot == null || SuperElecLine == null || SuperElecLine.m_Watt <= 0f)
                return false;

            float value = SystemMgr.GetGBotMaxFatigue() - bot.m_Fatigue;

            if (value == 0f)
                return true;

            //补满电力
            if (value > 0 && SuperElecLine.UseWatt(-1, -value))
            {
                bot.FatigueUpate(value);

                if (bot.m_CharState == CharState.Injury)
                {
                    bot.ImFatigueSet(0);

                    bot.SetCharState(CharState.None);

                    bot.SetAniState(AniState.Idle, "Idle_GBot", true, true);

                    Debug.LogWarning($"机械 {bot.m_UnitName} 已倒地！");
                }

                Debug.LogWarning($"机械 {bot.m_UnitName} 通过连接奥米伽-7补充了 {value} 体力！");

                return true;
            }
            //补充少量电力
            else if (bot.m_Fatigue < 10)
            {
                value = SuperElecLine.m_Watt > 10 ? 10 : SuperElecLine.m_Watt;

                if (SuperElecLine.UseWatt(-1, -value))
                {
                    bot.FatigueUpate(10);

                    Debug.LogWarning($"机械 {bot.m_UnitName} 通过连接奥米伽-7补充了 {value} 体力！");

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 机械体力更新
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GBot_FatigueUpate(GBot __instance, float value)
        {
            if (!CustomCharInfoIsActive("AMJ7_LZJX") || SuperElecLine == null || __instance.m_ImFatigue != 0 || value > 0f)
                return true;

            float num = 1f + (__instance.m_Buff.GetBuffValue(C_Buff.SLP_Up) + __instance.m_Buff.GetBuffValue(C_Buff.SLP_Down));

            if (num != 0f)
            {
                if (num < 0f)
                {
                    num = 0.01f;
                }
                value *= num;
            }

            //维持消耗
            if (!SuperElecLine.UseWatt(-1, value))
                return true;

            FillUpGbotPower(__instance);

            return false;
        }
        /// <summary>
        /// 电网添加耗电信息
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ElecLine_Info_AddConnectUseBuild(ElecLine_Info __instance, int _id, float _value)
        {
            if (!CustomCharInfoIsActive("AMJ7_LZJX") || _id < 0)
                return true;

            __instance.m_HourUseWatt += _value;

            return false;
        }
        /// <summary>
        /// 电网获得电力
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_useid"></param>
        /// <param name="_value"></param>
        /// <returns></returns>
        public static void ElecLine_Info_AddWatt(ElecLine_Info __instance, float _value)
        {
            if (!CustomCharInfoIsActive("AMJ7_LZJX") || _value <= 0 || SuperElecLine == null)
                return;

            //自动为机械鼠补充电力
            foreach (GBot bot in UnitMgr.List_GBot)
            {
                if (!FillUpGbotPower(bot))
                    break;
            }
        }

        /// <summary>
        /// 超级电网
        /// </summary>
        static ElecLine_Info SuperElecLine = null;
        /// <summary>
        /// 量子电网
        /// </summary>
        static void AMJ7_LZDW_Effect()
        {
            SuperElecLine = null;

            if (!CustomCharInfoIsActive("AMJ7_LZDW"))
                return;

            CombineAllElecLine();

            BuildingMgr.RefreshElecUseBuilding();

            Debug.LogWarning($"量子电网已启动！");
        }
        /// <summary>
        /// 合并所有电网
        /// </summary>
        static void CombineAllElecLine()
        {
            List<ElecLine_Info> elecLineList = BuildingMgr.List_ElecInfo;

            int count = elecLineList.Count;

            while (elecLineList.Count > 1)
            {
                BuildingMgr.MergeTwoElecLine(elecLineList[0], elecLineList[elecLineList.Count - 1]);
            }

            Debug.Log($"超级电网合并完成，共合并 {count} 电网");

            if (count == 0)
                return;

            SuperElecLine = elecLineList[0];

            Debug.Log($"总设备数量：{SuperElecLine.List_ID.Count}");
            Debug.Log($"电池数量：{SuperElecLine.Dic_Storage.Count}");
            Debug.Log($"发电机数量：{SuperElecLine.Dic_Dynamo.Count}");
            Debug.Log($"耗电设备数量：{SuperElecLine.Dic_UseBuild.Count}");
            Debug.Log($"当前电力：{SuperElecLine.m_Watt}");
            Debug.Log($"最大电力：{SuperElecLine.m_MaxWatt}");
            Debug.Log($"每小产出：{SuperElecLine.m_MakeWatt}");
            Debug.Log($"每小时消耗：{SuperElecLine.m_HourUseWatt}");
        }
        /// <summary>
        /// 基类建筑电路检查
        /// </summary>
        /// <param name="__instance"></param>
        public static void Building_WireCheck(Building __instance, bool _use, ref bool __result)
        {
            SuperElecLineWireCheck(__instance, _use, ref __result);
        }
        /// <summary>
        /// 电力建筑电路检查
        /// </summary>
        /// <param name="__instance"></param>
        public static void Building_ElecMasonry_WireCheck(Building_ElecMasonry __instance, bool _use, ref bool __result)
        {
            SuperElecLineWireCheck(__instance, _use, ref __result);
        }
        /// <summary>
        /// 电力物流建筑电路检查
        /// </summary>
        /// <param name="__instance"></param>
        public static void Building_ElecCarrierStation_WireCheck(Building_ElecCarrierStation __instance, bool _use, ref bool __result)
        {
            SuperElecLineWireCheck(__instance, _use, ref __result);
        }
        /// <summary>
        /// 电力舞台建筑电路检查
        /// </summary>
        /// <param name="__instance"></param>
        public static void Building_ElecBandstand_WireCheck(Building_ElecBandstand __instance, bool _use, ref bool __result)
        {
            SuperElecLineWireCheck(__instance, _use, ref __result);
        }
        /// <summary>
        /// 超级电网电路检查
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_use"></param>
        /// <param name="__result"></param>
        static void SuperElecLineWireCheck(Building __instance, bool _use, ref bool __result)
        {
            if (!CustomCharInfoIsActive("AMJ7_LZDW") || SuperElecLine == null || __result)
                return;

            if (!__instance.m_Activation || __instance.m_BuildState == BuildState.NeedRepair || __instance.m_BuildState == BuildState.NeedGround || __instance.m_BuildState == BuildState.IsFlood)
                return;

            List<int> idList = SuperElecLine.Dic_Storage.Keys.ToList();

            Building_Battery building_Battery = idList.Count > 0 ? BuildingMgr.List_Battery.Find(t => t.m_ID.Equals(idList[0])) : null;

            if (building_Battery != null)
            {
                __instance.m_ElecWire.WireSet(__instance.Tf.position, building_Battery.Tf.position, building_Battery);

                BuildingMgr.ConnectUseBuild(__instance.m_ID, building_Battery.m_ID, __instance.m_Info.ElecCost);

                if (_use && building_Battery.UseWatt(__instance.m_ID, __instance.m_Info.ElecCost))
                {
                    if (__instance.m_BuildAlarm.m_State == BuildState.NoElec || __instance.m_BuildAlarm.m_State == BuildState.NoBattery)
                    {
                        __instance.m_BuildState = BuildState.Basic;

                        __instance.AlarmSet(BuildState.Basic);
                    }
                    __instance.m_ElecNum = 1;

                    __result = true;

                    Debug.LogWarning($"{__instance.m_CustomName} 处于电网范围外，已单独加入超级电网！");
                }
            }
        }
        /// <summary>
        /// 获取周围电网（针对添加时）
        /// </summary>
        /// <param name="__instance"></param>
        public static bool BuildingMgr_GetFourDir_ElecGroup(ElecPort _port, ref List<ElecLine_Info> __result)
        {
            if (!CustomCharInfoIsActive("AMJ7_LZDW") || SuperElecLine == null)
                return true;

            __result = new List<ElecLine_Info> { SuperElecLine };

            //Debug.LogWarning($"端口 {_port.m_PortType} 获取超级电网");

            return false;
        }
        /// <summary>
        /// 建筑电力删除连接检查
        /// </summary>
        /// <param name="__instance"></param>
        public static bool BuildingMgr_DeleteConnectCheck(BuildingMgr __instance, int _id, List<ElecPort> _list_port)
        {
            if (!CustomCharInfoIsActive("AMJ7_LZDW") || SuperElecLine == null)
                return true;

            for (int i = 0; i < _list_port.Count; i++)
            {
                Vector2Int vector2Int = new Vector2Int(_list_port[i].m_X, _list_port[i].m_Y);
                if (__instance.Dic_PortTileMap.ContainsKey(vector2Int))
                {
                    __instance.Dic_PortTileMap.Remove(vector2Int);
                }
                __instance.RefreshWire(vector2Int);
            }

            ElecLine_Info elecLine = __instance.SearchElecInfo(_id);

            if (elecLine != null)
                elecLine.RemoveKey(_id);

            List<int> list = (elecLine != null) ? elecLine.FindAnotherStorageList(_id) : new List<int>() { };

            Debug.LogWarning($"({_id}) 所在电网 {(elecLine != null ? elecLine.m_CustomName : "无")} 与 {list.Count} 电池相连，触发删除检测，当前共 {BuildingMgr.List_ElecInfo.Count} 电网");

            //CombineAllElecLine();

            return false;
        }
        /// <summary>
        /// 电网使用电力
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_useid"></param>
        /// <param name="_value"></param>
        /// <returns></returns>
        public static void ElecLine_Info_UseWatt(ElecLine_Info __instance, ref float _value)
        {
            if (!TryGetCustomValue("奥米伽-7", "AMJ7_LZDW", out _, out _, out float value1, out _) || _value > 0f)
                return;

            float costRatio = 1 - AMJ7_PDI / value1 / 100f;

            costRatio = costRatio < 0f ? 0f : costRatio;

            _value *= costRatio;

            //Debug.Log($"量子电网消耗减免 {1 - costRatio}，消耗值 {_value}");
        }

        /// <summary>
        /// 电气场地
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="overflowPower"></param>
        static void PKQ_DQCD_Effect(T_Citizen citizen, float overflowPower)
        {
            if (overflowPower <= 0f)
                return;

            string key = "PKQ_DQCD";

            if (!ActiveCustomSpecialUnit || !CitizenHaveCharacterInfo(citizen, key, out CharacterInfo info))
                return;

            citizen.m_Fatigue += overflowPower;

            float value = citizen.m_Dex * info.EffectValue_A / 100f;

            int time = (int)(overflowPower / info.EffectValue_B);

            for (int i = 0; i < Citizens.Count; i++)
            {
                UpdateSpecialStateToUnit(Citizens[i], C_Buff.SpdUp, key, value, Citizens[i] == citizen, true, time == 0 ? 1 : time);
            }

            Debug.Log($"{citizen.m_UnitName} 溢出电力 {overflowPower} 获得状态 {info.T_Name}");
        }
        /// <summary>
        /// 十万伏特
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="info"></param>
        static void PKQ_SWFT_Effect(T_Citizen citizen, Building_ThermalGenerator building)
        {
            if (building == null || !TryGetCustomCharInfo("PKQ_SWFT", out CustomCharInfo info))
                return;

            int value = IntProbability;
            float thrValue = (citizen.m_Power + citizen.m_Int) * info.value1;

            Debug.Log($"十万伏特检查 {value <= thrValue}({value} {thrValue})");

            //十万伏特
            if (value > thrValue)
                return;

            int ratio = RandomInt(citizen.m_Int / (int)info.value1, citizen.m_Int * (int)info.value1 + 1);
            float power = citizen.m_Power * ratio;

            ElecLine_Info elecLine_Info = GameMgr.Instance._BuildingMgr.SearchElecInfo(building.m_ID);
            if (elecLine_Info == null)
            {
                Debug.LogWarning($"皮卡丘发电失败：建筑 {building.m_CustomName} 未连接电网");
                return;
            }

            //溢出的电力
            float overflowPower = elecLine_Info.m_Watt + power - elecLine_Info.m_MaxWatt;
            elecLine_Info.AddWatt(power);
            IndividualStatisticsManager.Instance.Add(GameMgr.Instance._SysMgr.m_Day, building.m_ID, power, new string[]
            {
            "Electricity",
            "Product",
            "Value"
            });

            Debug.LogWarning($"皮卡丘触发了{ratio}倍十万伏特，产生了 {power} 电力");

            PKQ_DQCD_Effect(citizen, overflowPower);

            //损坏
            if (IntProbability <= info.value2 - citizen.m_Int)
            {
                building.m_CurHP = 0f;

                building.SetNeedRepair(true);

                Debug.LogWarning($"皮卡丘的十万伏特损坏了建筑 {building.m_CustomName}");
            }
        }
        /// <summary>
        /// 建筑工作进度更新后
        /// 工作完成后
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="d_time"></param>
        public static void MasonryInfo_WorkUpdate_Postfix(MasonryInfo __instance, ref float d_time)
        {
            if (!ActiveCustomSpecialUnit || __instance.m_CurTime != 0 || d_time == 0)
                return;

            Building building = __instance.m_Building;

            if (building == null)
                return;

            T_Citizen worker = building.m_Master;

            if (worker == null)
                return;

            Debug.Log($"{building.m_Info.T_Name}({building.m_Info.Name}) 完成工作 {d_time}");

            //皮卡丘在鼠力发电站完成工作时
            if (building.m_Info.Name == BuildingName.ManpowerGenerator && CitizenIsSpecialUnit(worker, "皮卡丘") && CustomCharInfoIsActive("PKQ_SWFT", out _))
            {
                PKQ_SWFT_Effect(worker, building as Building_ThermalGenerator);

                Debug.LogWarning($"{building.m_Info.T_Name}({building.m_Info.Name})({building.GetType()}/{building is Building_ThermalGenerator}) {worker.m_UnitName}完成了发电工作 {d_time}，当前力量经验 {worker.m_PowerExp}");
            }
        }

        /// <summary>
        /// 七探蛇盘枪
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="unit"></param>
        /// <param name="info"></param>
        static void ZY_QTSPQ_Effect(T_Citizen citizen, GameUnit unit, CharacterInfo info)
        {
            //击退
            unit.Knockback(unit.Tf.position, false, 0, info.EffectValue_A);

            //减少50%移速
            UpdateSpecialStateToUnit(unit, C_Buff.SpdDown, "QTSPQ", 0.5f);

            Debug.Log($"{citizen.name} 击退了 {unit.name}");
        }
        /// <summary>
        /// 龙胆
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="usedDetection"></param>
        static void ZY_LD_Effect(T_Citizen citizen, CharacterInfo info = null)
        {
            string key = "ZY_LD";

            citizen.m_Buff.RefKill(key);

            if (!ActiveCustomSpecialUnit || (info == null && !CitizenHaveCharacterInfo(citizen, key, out info)))
                return;

            UpdateSpecialStateToUnit(citizen, C_Buff.SpdUp, key, info.EffectValue_A);
            UpdateSpecialStateToUnit(citizen, C_Buff.Dodge, key, info.EffectValue_B, false, false);

            Debug.Log($"{citizen.m_UnitName} 获得状态 {info.T_Name}");
        }

        /// <summary>
        /// 蘑菇之力
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="time"></param>
        static void DZ_MGZL_Effect(T_Citizen citizen, int time)
        {
            if (time == 0)
                return;

            string key = "DZ_MGZL";

            int value = citizen.m_Buff.GetRestHour(key);

            time = time < value ? value : time;

            UpdateSpecialStateToUnit(citizen, C_Buff.SLP_Down, key, -0.3f, true, true, time);
            UpdateSpecialStateToUnit(citizen, C_Buff.ProductivityUp, key, 0.1f, false, false, time);
            UpdateSpecialStateToUnit(citizen, C_Buff.SpdUp, key, 0.1f, false, false, time);
        }
        /// <summary>
        /// 应用食物或生活用品效果
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="t_info"></param>
        public static void T_Citizen_ApplyFoodOrLife_ResAbility(T_Citizen __instance, TileInfo t_info)
        {
            if (!CustomCharInfoIsActive("DZ_MGZL"))
                return;

            //Debug.Log($"{__instance.m_UnitName} 使用了 {t_info.T_Name}({t_info.m_TileType})");

            int time = 0;

            if (t_info.m_TileType == TileType.Mushroom)
                time = 18;
            else if (t_info.m_TileType == TileType.GrilledMushroom)
                time = 24;
            else if (t_info.m_TileType == TileType.Steak)
                time = 36;

            DZ_MGZL_Effect(__instance, time);
        }

        /// <summary>
        /// 蘑菇教主值
        /// </summary>
        static float DZMGJZ_Value
        {
            get
            {
                float value = ActiveCustomSpecialUnit && TryGetCustomValue("大正", "DZ_MGJZ", out CustomSpecialUnit unit, out T_Citizen citizen, out float value1, out float value2) ? 1f + (value1 + (citizen.m_Int - unit.wit) * value2) / 100f : 1f;

                value = value < 0f ? 0f : value;

                return value;
            }
        }
        /// <summary>
        /// 建筑工作进度更新前
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="d_time"></param>
        public static void MasonryInfo_WorkUpdate_Prefix(MasonryInfo __instance, ref float d_time)
        {
            if (!ActiveCustomSpecialUnit)
                return;

            //更新蘑菇农场的工作效率
            if (__instance.m_Building.m_Info.Name == BuildingName.MushroomFarm && CustomCharInfoIsActive("DZ_MGJZ", out _))
                d_time *= DZMGJZ_Value;
        }

        /// <summary>
        /// 梨园值2
        /// </summary>
        static float LLJLY_Value2
        {
            get
            {
                return TryGetCustomValue("李隆基", "LLJ_LY", out CustomSpecialUnit _, out T_Citizen citizen, out _, out float value2) ? float.Parse((citizen.m_Int * value2 / 100f).ToString("F2")) : 0f;
            }
        }
        /// <summary>
        /// 梨园值1
        /// </summary>
        static float LLJLY_Value1
        {
            get
            {
                float value = 0f;

                if (TryGetCustomValue("李隆基", "LLJ_LY", out CustomSpecialUnit _, out T_Citizen citizen, out float value1, out _))
                {
                    float intValue = citizen.m_Int - value1 < 1f ? 1f : citizen.m_Int - value1;

                    value = 1f - intValue / (intValue + 1);
                }

                return float.Parse(value.ToString("F2"));
            }
        }
        /// <summary>
        /// 梨园
        /// </summary>
        /// <param name="customInfo"></param>
        static void LLJ_LY_Effect(T_Citizen citizen = null)
        {
            string key = "LLJ_LY";

            bool canUse = CustomCharInfoIsActive(key);

            List<T_Citizen> list = citizen != null ? new List<T_Citizen> { citizen } : Citizens;

            //多状态放在Effect进行迭代
            for (int i = 0; i < list.Count; i++)
            {
                list[i].m_Buff.RefKill(key);

                //允许清理
                if (!canUse)
                    continue;

                UpdateSpecialStateToUnit(list[i], C_Buff.ProductivityUp, key, LLJLY_Value2);
                UpdateSpecialStateToUnit(list[i], C_Buff.FUN_Down, key, LLJLY_Value1, false, false);
            }
        }

        /// <summary>
        /// 开元盛世
        /// </summary>
        static int LLJ_KYSS_Value
        {
            get
            {
                int value = TryGetCustomValue("李隆基", "LLJ_KYSS", out CustomSpecialUnit unit, out T_Citizen citizen, out float value1, out _) ? citizen.m_Int - unit.wit + Mathf.FloorToInt(ProsperityLevel / value1) : 0;

                return value;
            }
        }
        /// <summary>
        /// 获得最大访客数
        /// </summary>
        /// <param name="_name"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool Helpers_Get_MaximumGuestNum(BuildingName _name, ref int __result)
        {
            if (!ActiveCustomSpecialUnit)
                return true;

            if (_name == BuildingName.Atelier || _name == BuildingName.HairShop || _name == BuildingName.Laundry || _name == BuildingName.Hospital || _name == BuildingName.MassageBed || _name == BuildingName.Toilet || _name == BuildingName.GuardPost || _name == BuildingName.FleaCleaner)
            {
                __result = 1 + LLJ_KYSS_Value;
            }
            else if (_name == BuildingName.BugRacingTrack)
            {
                __result = 4;
            }
            else
                __result = 2 + LLJ_KYSS_Value;

            return false;
        }

        /// <summary>
        /// 商圣
        /// </summary>
        static float BGSS_Value
        {
            get
            {
                return TryGetCustomValue("白圭", "BG_SS", out CustomSpecialUnit _, out T_Citizen citizen, out float value1, out _) ? citizen.m_Int * value1 / 100f : 0f;
            }
        }
        /// <summary>
        /// 能以取予值
        /// </summary>
        static float BGNYQY_Value
        {
            get
            {
                float value = 0f;

                if (TryGetCustomValue("白圭", "BG_NYQY", out CustomSpecialUnit _, out T_Citizen citizen, out float value1, out _))
                {
                    float intValue = citizen.m_Int - value1 < 1f ? 1f : citizen.m_Int - value1;

                    value = intValue / (intValue + 1) - 1f;
                }

                return value;
            }
        }
        //static string baseValueText = "";
        //static string nyqyUpdateValueText = "";
        //static string ssUpdateValueText = "";
        /// <summary>
        /// 获得进口价格
        /// </summary>
        /// <param name="price"></param>
        /// <param name="nowRelations"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool DiplomaticCountryResourceData_TradeCountryToMyKingdomPrice(float price, int nowRelations, TileInfo ____info, ref float __result)
        {
            if (!ActiveCustomSpecialUnit)
                return true;

            if (____info == null)
            {
                Debug.LogWarning("贸易物品获取失败！");

                return true;
            }

            float baseValue = price * (1.2f - (nowRelations - 70) / 200f);

            //baseValueText = baseValue.ToString();

            //nyqyUpdateValueText = "";

            //ssUpdateValueText = $"{SymbolText}{ColorText_F}{baseValue * GetSSValue(____info)}{ColorText_B}";

            float value = 1 + GetSSValue(____info);

            __result = baseValue * value;

            Debug.Log($"{____info.T_Name} 进口原价为 {baseValue} 最终进口价格为 {__result}，影响系数为 {value}");

            return false;
        }
        /// <summary>
        /// 获得出口价格
        /// </summary>
        /// <param name="price"></param>
        /// <param name="nowRelations"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool DiplomaticCountryResourceData_TradeMyKingdomToCountryPrice(float price, int nowRelations, TileInfo ____info, ref float __result)
        {
            if (!ActiveCustomSpecialUnit)
                return true;

            if (____info == null)
            {
                Debug.LogWarning("贸易物品获取失败！");
                return true;
            }

            float baseValue = price * (0.8f + (nowRelations - 70) / 200f);

            //baseValueText = baseValue.ToString();

            //nyqyUpdateValueText = $" - <color=#C83232>{baseValue * Mathf.Abs(BGNYQY_Value)}</color>";

            //ssUpdateValueText = $"{SymbolText}{ColorText_F}{baseValue * GetSSValue(____info)}{ColorText_B}";

            float value = 1 + BGNYQY_Value + GetSSValue(____info);

            __result = baseValue * value;

            Debug.Log($"{____info.T_Name} 出口原价为 {baseValue} 最终出口价格为 {__result}，影响系数为 {value}");

            return false;
        }
        /// <summary>
        /// 价格受商圣影响，-1：无，0：增，1：减
        /// </summary>
        static int priceIsUpdateBySS = -1;
        static string SymbolText { get { return priceIsUpdateBySS == -1 ? "" : priceIsUpdateBySS == 0 ? " (涨)" : " (跌)"; } }
        static string ColorText_F { get { return priceIsUpdateBySS == 0 ? "<color=#1E8A00>" : priceIsUpdateBySS == 1 ? "<color=#C83232>" : ""; } }
        static string ColorText_B { get { return priceIsUpdateBySS > -1 ? "</color>" : ""; } }
        /// <summary>
        /// 获得商圣影响值
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        static float GetSSValue(TileInfo info)
        {
            if (info == null)
                return 0f;

            float ssValue = BGSS_Value, value = 0f;

            //日用品春降夏涨
            if (info.Category == ResCateogry.Life)
                value = WeatherMgr.m_SeasonState == SeasonState.Spring ? -ssValue : WeatherMgr.m_SeasonState == SeasonState.Summer ? ssValue : 0f;
            //食物秋降冬涨
            else if (info.Category == ResCateogry.Food)
                value = WeatherMgr.m_SeasonState == SeasonState.Fall ? -ssValue : WeatherMgr.m_SeasonState == SeasonState.Winter ? ssValue : 0f;

            priceIsUpdateBySS = value == 0 ? -1 : value > 0 ? 0 : 1;

            Debug.Log($"当前季节 {WeatherMgr.m_SeasonState} 贸易物品 {info.T_Name} 价格浮动 {value}({ssValue})");

            return value;
        }
        /// <summary>
        /// 商业值成长阈值
        /// </summary>
        static readonly int comValueGrowthThreshold = 1000;
        /// <summary>
        /// 城市最大繁荣值
        /// </summary>
        static readonly float maxCountryProsperityValue = 10000;
        /// <summary>
        /// 商业值增长最小系数值
        /// </summary>
        static readonly float comGrowthMinValue = 0.1f;
        /// <summary>
        /// 城市商业值数据
        /// </summary>
        static Dictionary<string, float> CountryCommercialityDatas = new Dictionary<string, float>();
        /// <summary>
        /// 贸易完成事件
        /// </summary>
        /// <param name="result"></param>
        /// <param name="__result"></param>
        public static void DiplomaticMgr_OnTradeResultEvent_BGNYQY(TradeResult result, TradeReceive __result)
        {
            if (!CustomCharInfoIsActive("BG_NYQY") || __result.TradeReceiveState != TradeReceiveState.Success)
                return;

            DiplomaticCountryTradeSheetData sheet = result.Sheet;

            DiplomaticCountryData country = sheet.CountryData;

            TypeTrade typeTrade = sheet.TypeTrade;

            //获得贸易价值
            float value = typeTrade == TypeTrade.Country_To_Hometown ? sheet.TotalTradeCountryToHometownPrice() : sheet.TotalTradeHometownToCountryPrice();

            //最终贸易价值
            value *= country.TypeMoney == TypeMoney.Dar ? 10f : 1f;

            bool haveComValue = CountryCommercialityDatas.TryGetValue(country.Key, out float comValue);

            //商业增长系数
            double factorValue = GetComValueGrowthFactor(country.NowProsperityValue);

            //商业增长值
            float comAddValue = (float)(value * factorValue);

            //累计商业值
            comValue += comAddValue;

            //繁荣增长值
            int addValue = Mathf.FloorToInt(comValue / comValueGrowthThreshold);

            //剩余商业值
            float remainingValue = comValue - comValueGrowthThreshold * addValue;

            if (!haveComValue)
                CountryCommercialityDatas.Add(country.Key, remainingValue);
            else
                CountryCommercialityDatas[country.Key] = remainingValue;

            Debug.LogWarning($"贸易完成，贸易价值 {value} 增长系数 {factorValue.ToString("f4")}，目标城市 {country.Name} 获得 {comAddValue}(累计 {comValue} - 消耗 {comValueGrowthThreshold * addValue} = 剩余 {remainingValue}) 商业值，繁荣提升了 {addValue} 点");
        }
        /// <summary>
        /// 获得商业值增长系数
        /// </summary>
        /// <param name="prosperityValue"></param>
        /// <returns></returns>
        static float GetComValueGrowthFactor(float prosperityValue)
        {
            prosperityValue = prosperityValue > maxCountryProsperityValue ? maxCountryProsperityValue : prosperityValue;

            return comGrowthMinValue + (1 - comGrowthMinValue) * (maxCountryProsperityValue - prosperityValue) / maxCountryProsperityValue;
        }

        /// <summary>
        /// 牛车值
        /// </summary>
        static float WHNC_Value
        {
            get
            {
                float value = ActiveCustomSpecialUnit && TryGetCustomValue("王亥", "WH_NC", out CustomSpecialUnit unit, out T_Citizen citizen, out float value1, out float value2) ? 1f - (value1 + (citizen.m_Dex - unit.dex) * value2) / 100f : 1f;

                value = value < 0f ? 0f : value > 1f ? 1f : value;

                return value;
            }
        }
        /// <summary>
        /// 设置城市贸易距离
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="tInstance"></param>
        public static void DiplomaticData_SetTerrainTotalDistance(DiplomaticWorldTerrainEntity tInstance)
        {
            if (!ActiveCustomSpecialUnit)
                return;

            int dis = tInstance.TotalDistanceValue;

            int newDis = Mathf.FloorToInt(dis * WHNC_Value);

            newDis = newDis <= 0 ? 1 : newDis;

            tInstance.SetTotalDistance(newDis);

            //foreach (DiplomaticCountryData country in ____countryDic.Values)
            //{ 
            //    country
            //}

            Debug.LogWarning($"DiplomaticData: 设置目标城市 {tInstance.ID} 距离 {newDis}/{dis}");
        }

        /// <summary>
        /// 商祖值
        /// </summary>
        static int WHSZ_Value
        {
            get
            {
                return ActiveCustomSpecialUnit && TryGetCustomValue("王亥", "WH_SZ", out CustomSpecialUnit unit, out T_Citizen citizen, out float value1, out _) ? Mathf.FloorToInt(citizen.m_Int - unit.wit + ProsperityLevel / value1) : 0;
            }
        }
        /// <summary>
        /// 获得最大贸易协议数量
        /// </summary>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool DiplomaticCountryData_MaxTradeAgreementCount(ref int __result)
        {
            if (!ActiveCustomSpecialUnit)
                return true;

            __result = 3 + WHSZ_Value;

            return false;
        }

        /// <summary>
        /// 五禽戏
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="check"></param>
        static void HT_WQX_Effect(T_Citizen citizen, bool check = true)
        {
            string key = "HT_WQX";

            citizen.m_Buff.RefKill(key);

            if (check && !CustomCharInfoIsActive(key))
                return;

            UpdateSpecialStateToUnit(citizen, C_Buff.MaxHP_Up, key, 50);
            UpdateSpecialStateToUnit(citizen, C_Buff.ProductivityUp, key, 0.2f, false, false);
            UpdateSpecialStateToUnit(citizen, C_Buff.SpdUp, key, 0.1f, false, false);
        }

        /// <summary>
        /// 岳家军
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="check"></param>
        static void YF_YJJ_Effect(T_Citizen citizen, bool check = true)
        {
            string key = "YF_YJJ";

            citizen.m_Buff.RefKill(key);

            if ((check && !CustomCharInfoIsActive(key)) || !CitizenIsSolider(citizen))
                return;

            UpdateSpecialStateToUnit(citizen, C_Buff.ATK_Up, key, 5);
            UpdateSpecialStateToUnit(citizen, C_Buff.DEF_Up, key, 3, false, false);
            UpdateSpecialStateToUnit(citizen, C_Buff.SpdUp, key, 0.3f, false, false);
        }
        /// <summary>
        /// 岳家枪
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="dmg"></param>
        /// <param name="name"></param>
        static void YF_YJQ_Effect(T_Citizen citizen, float dmg, CharacterInfo info, string name)
        {
            float ratio = info != null ? info.EffectValue_A : 30f;

            float healValue = Mathf.Abs(dmg * citizen.m_Dex / ratio);

            citizen.Heal(healValue, true);

            Debug.Log($"{citizen.name} 攻击了 {name}，造成 {-dmg} 伤害，恢复 {healValue} 生命");
        }

        /// <summary>
        /// 市民更新职业
        /// 岳家军
        /// </summary>
        public static void T_Citizen_JobSet(T_Citizen __instance)
        {
            if (!ActiveCustomSpecialUnit)
                return;

            YF_YJJ_Effect(__instance);
        }

        /// <summary>
        /// 市民是士兵
        /// </summary>
        /// <param name="citizen"></param>
        /// <returns></returns>
        static bool CitizenIsSolider(T_Citizen citizen)
        {
            if (citizen == null || citizen.m_Job == null)
                return false;

            BuildingName buildingName = citizen.m_Job.m_Info.Name;

            //训练营地、兵营、战斗训练所、战争神殿、守护者大厅
            return buildingName == BuildingName.TrainingCamp || buildingName == BuildingName.Barrack || buildingName == BuildingName.CombatAcademy || buildingName == BuildingName.DruidCamp || buildingName == BuildingName.GuardianTemple;
        }

        /// <summary>
        /// 市民近战攻击
        /// 岳家枪 七探蛇盘枪
        /// </summary>
        public static void T_Citizen_SwdAtk_Call(T_Citizen __instance)
        {
            if (!ActiveCustomSpecialUnit)
                return;

            bool yjq = CitizenHaveCharacterInfo(__instance, "YF_YJQ", out CharacterInfo yqjInfo), qtspq = CitizenHaveCharacterInfo(__instance, "ZY_QTSPQ", out CharacterInfo qtspqInfo);

            if (!yjq && !qtspq)
                return;

            GameUnit targetUnit = __instance.m_TargetUnit;

            float dmg = __instance.GetDmg();

            if (targetUnit != null && __instance.m_AtkBox.IsCollide(targetUnit, false) && targetUnit.m_CharState != CharState.Death && targetUnit.m_CharState != CharState.Injury)
            {
                //装备长枪时
                if (__instance.m_WeaponName == WeaponName.Spear || __instance.m_WeaponName == WeaponName.AdvancedSpear || __instance.m_WeaponName == WeaponName.FlagSpear || __instance.m_WeaponName == WeaponName.SkirmisherSpear)
                {
                    for (int i = 0; i < UnitMgr.List_AllEnemy.Count; i++)
                    {
                        GameUnit unit = UnitMgr.List_AllEnemy[i];

                        bool hitTarget = __instance.m_AtkBox.IsCollide(unit, false);

                        //命中溅射单位
                        if (unit != targetUnit && hitTarget)
                        {
                            float rangeDmg = dmg * 0.5f;

                            if (yjq)
                                YF_YJQ_Effect(__instance, rangeDmg, yqjInfo, unit.name);
                            else if (qtspq)
                                ZY_QTSPQ_Effect(__instance, unit, qtspqInfo);

                            unit.BeAttacked(-rangeDmg, Unit_Attacekd_Tag.OurTeam, __instance.m_ID);
                        }
                    }
                }

                if (yjq)
                    YF_YJQ_Effect(__instance, dmg, yqjInfo, targetUnit.name);
                else if (qtspq)
                    ZY_QTSPQ_Effect(__instance, targetUnit, qtspqInfo);

                Debug.Log($"{__instance.name} 攻击了 {targetUnit.name}({targetUnit.GetType()})");
            }
        }

        /// <summary>
        /// 秦律值
        /// </summary>
        static int SYQL_Value
        {
            get
            {
                return ActiveCustomSpecialUnit && TryGetCustomValue("商鞅", "SY_QL", out CustomSpecialUnit unit, out T_Citizen citizen, out float value1, out _) ? Mathf.FloorToInt(citizen.m_Int - unit.wit + ProsperityLevel / value1) : 0;
            }
        }
        /// <summary>
        /// 秦律：更新法典数量
        /// </summary>
        static void SY_QL_Effect()
        {
            if (!EnsureProsperityBaseline())
                return;

            int bonus = CustomCharInfoIsActive("SY_QL") ? SYQL_Value : 0;
            int[] values = ProsperityBaselinePolicy.ApplyBonus(
                ProsperityDB.Select(info => info.PolicyNum).ToArray(),
                bonus);

            for (int i = 0; i < DBMgr.List_ProsperityDB.Count; i++)
            {
                DBMgr.List_ProsperityDB[i].PolicyNum = values[i];
            }
        }
        /// <summary>
        /// 垦草令的值
        /// </summary>
        static int KCL_Value
        {
            get
            {
                int count = Citizens.Count;

                if (!ActiveCustomSpecialUnit || count == 0)
                    return 0;

                if (!TryGetCustomValue("商鞅", "SY_KCL", out _, out _, out float value1, out float value2))
                {
                    value1 = 10;

                    value2 = 30;
                }

                int ratio = Mathf.FloorToInt(SttMgr.m_FoodUI.m_FoodNum / count);

                int value = Mathf.FloorToInt(ratio / value1);

                return (int)(value > value2 ? value2 : value);
            }
        }
        /// <summary>
        /// 更新食物
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static void FoodUI_AllFood_Update()
        {
            int value = KCL_Value;

            //垦草令
            if (NeedUpdatePDIWithCustomCharInfo("SY_KCL", value, out CustomCharInfo info))
                UpdateSpecialStateToAllCitizen(info.c_Buff, info.name, value, false);
        }

        /// <summary>
        /// 上一次的值
        /// </summary>
        static Dictionary<string, float> preValueDic = new Dictionary<string, float>();
        /// <summary>
        /// 奈奈的关爱的值
        /// </summary>
        static int NNBen_Value
        {
            get
            {
                return ActiveCustomSpecialUnit && TryGetCustomValue("奈奈酱", "NaiNai_Benevolence", out CustomSpecialUnit unit, out T_Citizen citizen, out float value1, out _) ? GetPDIValue(citizen, value1) : 0;
            }
        }
        /// <summary>
        /// 更新三维
        /// </summary>
        /// <param name="__instance"></param>
        public static void GameUnit_UpdatePDI_Post()
        {
            //奈奈的希望
            float value = NNBen_Value;
            if (NeedUpdatePDIWithCustomCharInfo("NaiNai_Benevolence", value, out CustomCharInfo info))
                UpdateSpecialStateToAllCitizen(info.c_Buff, info.name, value, false);

            //秦律
            value = SYQL_Value;
            if (NeedUpdatePDIWithCustomCharInfo("SY_QL", value, out _))
                SY_QL_Effect();

            //梨园
            value = LLJLY_Value1 + LLJLY_Value2;
            if (NeedUpdatePDIWithCustomCharInfo("LLJ_LY", value, out _))
                LLJ_LY_Effect();

            //量子机械
            value = LZJX_Value;
            if (NeedUpdatePDIWithCustomCharInfo("AMJ7_LZJX", value, out _))
                AMJ7_LZJX_Effect();
        }

        #endregion

        #region 公共部分

        /// <summary>
        /// 需要更新自定义特性的三维值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="nowValue"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        static bool NeedUpdatePDIWithCustomCharInfo(string key, float nowValue, out CustomCharInfo info)
        {
            return CustomCharInfoIsActive(key, out info) && !IsSameValue(key, nowValue);
        }

        /// <summary>
        /// 是相同的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="nowValue"></param>
        /// <returns></returns>
        static bool IsSameValue(string key, float nowValue)
        {
            if (!preValueDic.TryGetValue(key, out float value))
            {
                preValueDic.Add(key, nowValue);

                return false;
            }
            else if (value != nowValue)
            {
                preValueDic[key] = nowValue;

                return false;
            }

            return true;
        }

        /// <summary>
        /// 市民恢复饱食度
        /// 联邦的希望
        /// </summary>
        /// <param name="__instance"></param>
        public static void T_Citizen_HungerUpdate(T_Citizen __instance, float value)
        {
            if (!ActiveCustomSpecialUnit || value < 0f || !CitizenHaveCharacterInfo(__instance, "LB_Hope", out CharacterInfo info))
                return;

            float happy = __instance.GetHappyValue();

            int minNum = GetPDIValue(__instance, info.EffectValue_B), maxNum = GetPDIValue(__instance, info.EffectValue_B / 1.5f), result = 0;

            for (int i = 0; i < maxNum; i++)
            {
                if (RandomFloat(0f, 100f) <= happy)
                    result += 1;
            }

            result = result < minNum ? minNum : result;

            Debug.Log($"{__instance.m_UnitName} 当前幸福 {happy} 产量 {result}");

            CreateTileObj(TileType.Gold, __instance.GetPos(), result);
        }

        /// <summary>
        /// 更新自身的特殊状态
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="name"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        static void UpdateSelfSpecialState(T_Citizen citizen, string name, float? num = null)
        {
            if (CitizenHaveCharacterInfo(citizen, name, out _))
                UpdateSpecialState(name, num, citizen);
        }

        /// <summary>
        /// 更新特殊状态
        /// </summary>
        static bool UpdateSpecialState(string name, float? num = null, T_Citizen citizen = null)
        {
            float value = 0f;

            C_Buff? cBuff = null;

            if (CustomCharInfoIsActive(name, out CustomCharInfo info))
            {
                value = num ?? info.value1;

                cBuff = info.c_Buff;
            }

            //单独更新
            if (citizen != null)
                UpdateSpecialStateToUnit(citizen, cBuff, name, value);
            //全体更新
            else
                UpdateSpecialStateToAllCitizen(cBuff, info.name, value);

            return true;
        }

        /// <summary>
        /// 为所有市民更新特殊状态
        /// </summary>
        /// <param name="c_Buff"></param>
        /// <param name="refName"></param>
        /// <param name="value"></param>
        /// <param name="time_hour"></param>
        /// <param name="category"></param>
        /// <param name="show"></param>
        static void UpdateSpecialStateToAllCitizen(C_Buff? c_Buff, string refName, float value, bool show = true, bool kill = true, int time_hour = -999, C_Buff_Category category = C_Buff_Category.None)
        {
            for (int i = 0; i < Citizens.Count; i++)
            {
                UpdateSpecialStateToUnit(Citizens[i], c_Buff, refName, value, show, kill, time_hour, category);
            }
        }

        /// <summary>
        /// 为单位更新状态
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="c_Buff"></param>
        /// <param name="refName"></param>
        /// <param name="value"></param>
        /// <param name="show"></param>
        /// <param name="time_hour"></param>
        /// <param name="category"></param>
        static void UpdateSpecialStateToUnit(GameUnit unit, C_Buff? c_Buff, string refName, float value, bool show = true, bool kill = true, int time_hour = -999, C_Buff_Category category = C_Buff_Category.None)
        {
            if (kill)
                unit.m_Buff.RefKill(refName);

            if (ActiveCustomSpecialUnit && c_Buff != null)
                unit.m_Buff.BuffRefSet((C_Buff)c_Buff, refName, category, value, time_hour, show);
        }

        /// <summary>
        /// 设置状态图标
        /// </summary>
        /// <param name="__instance"></param>
        public static void BuffIcon_IconSet(BuffIcon __instance, BuffInfo _info)
        {
            //Debug.Log($"{_info.T_Name} / {_info.ReferenceName}");

            if (!ActiveCustomSpecialUnit ||
                !TryGetCustomCharInfo(_info.ReferenceName, out CustomCharInfo customInfo))
                return;

            _info.T_Name = _info.ReferenceName;

            __instance.m_Spr.sprite = Func.Instance.LoadSprite(customInfo.iconKey);

            //Debug.Log($"加载了特殊状态 {_info.ReferenceName} 图标 {customInfo.iconKey}");
        }

        /// <summary>
        /// 获得三维值
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="ratio"></param>
        /// <returns></returns>
        static int GetPDIValue(T_Citizen citizen, float ratio = 1f)
        {
            return citizen != null ? Mathf.FloorToInt((citizen.GetPDI(PDI.Power) + citizen.GetPDI(PDI.Dex) + citizen.GetPDI(PDI.Int)) / ratio) : 0;
        }

        /// <summary>
        /// 是自定义特性
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static bool IsCustomCharInfo(string name)
        {
            return TryGetCustomCharInfo(name, out _);
        }

        /// <summary>
        /// 自定义特性是否启用
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static bool CustomCharInfoIsActive(string name)
        {
            return ActiveCustomSpecialUnit && CustomCharInfoIsActive(name, out _);
        }

        /// <summary>
        /// 自定义特性是否启用
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static bool CustomCharInfoIsActive(string name, out CustomCharInfo info)
        {
            return TryGetCustomCharInfo(name, out info) && info.IsActive;
        }

        /// <summary>
        /// 尝试获得自定义特性的值
        /// </summary>
        /// <param name="unitName"></param>
        /// <param name="charName"></param>
        /// <param name="unit"></param>
        /// <param name="user"></param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        static bool TryGetCustomValue(string unitName, string charName, out CustomSpecialUnit unit, out T_Citizen user, out float value1, out float value2)
        {
            user = null;

            value1 = 0f;

            value2 = 0f;

            if (TryGetSpecialUnit(unitName, out unit) && TryGetCustomCharInfo(charName, out CustomCharInfo info))
            {
                user = info.User;

                value1 = info.value1;

                value2 = info.value2;

                return user != null;
            }

            return false;
        }

        /// <summary>
        /// 尝试获得特殊鼠鼠
        /// </summary>
        /// <param name="unitName"></param>
        /// <param name="citizen"></param>
        /// <returns></returns>
        static bool TryGetSpecialCitizen(string unitName, out T_Citizen citizen)
        {
            citizen = null;

            return TryGetSpecialUnit(unitName, out CustomSpecialUnit unit) && SpecialCitizens.TryGetValue(unit.Name, out citizen);
        }

        /// <summary>
        /// 尝试获得特殊单位
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="customUnit"></param>
        /// <returns></returns>
        static bool TryGetSpecialUnit(T_Citizen citizen, out CustomSpecialUnit customUnit)
        {
            foreach (KeyValuePair<string, CustomSpecialUnit> keyValue in CustomSpecialUnitDatas)
            {
                //迭代市民的所有特性
                foreach (CharacterInfo info in citizen.List_CharInfoValue)
                {
                    //与自定义单位的特性匹配
                    if (info.Name.Equals(keyValue.Value.char1) || info.Name.Equals(keyValue.Value.char2))
                    {
                        customUnit = keyValue.Value;

                        Debug.LogWarning($"市民 {customUnit.name} 是特殊单位 ，特性1 {customUnit.char_1.Name}/{keyValue.Value.char1}，特性2 {customUnit.char_2.Name}/{keyValue.Value.char2}");

                        return true;
                    }
                }
            }

            customUnit = null;

            return false;
        }

        /// <summary>
        /// 尝试获得特殊单位
        /// </summary>
        /// <param name="name"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        static bool TryGetSpecialUnit(string name, out CustomSpecialUnit unit)
        {
            return CustomSpecialUnitDatas.TryGetValue(name, out unit);
        }

        /// <summary>
        /// 尝试获得自定义特性
        /// </summary>
        /// <param name="name"></param>
        /// <param name="charInfo"></param>
        /// <returns></returns>
        static bool TryGetCustomCharInfo(string name, out CustomCharInfo charInfo)
        {
            return CustomCharInfo.TryGetValue(name, out charInfo);
        }

        /// <summary>
        /// 尝试根据名称获得自定义特性
        /// </summary>
        /// <param name="t_name"></param>
        /// <param name="charInfo"></param>
        /// <returns></returns>
        static bool TryGetCustomCharInfoByTName(string t_name, out CustomCharInfo charInfo)
        {
            List<CustomCharInfo> list = CustomCharInfo.Select(t => t.Value).Where(t => t.t_name.Equals(t_name)).ToList();

            charInfo = list.Count > 0 ? list[0] : null;

            return list.Count > 0;
        }

        /// <summary>
        /// 获得图标地址
        /// </summary>
        /// <param name="_RefName"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool RefInfo_GetIconAddress(string _RefName, ref string __result)
        {
            if (!ActiveCustomSpecialUnit || !TryGetCustomCharInfo(_RefName, out CustomCharInfo customInfo))
                return true;

            __result = customInfo.iconKey;

            return false;
        }

        /// <summary>
        /// 获得状态名称
        /// </summary>
        /// <param name="_RefName"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool RefInfo_Get_T_Name(string _RefName, ref string __result)
        {
            if (!ActiveCustomSpecialUnit || !TryGetCustomCharInfo(_RefName, out CustomCharInfo customInfo))
                return true;

            __result = customInfo.t_name;

            return false;
        }

        /// <summary>
        /// 获取市民状态描述
        /// </summary>
        /// <param name="info"></param>
        public static bool CitizenBuff_RefInfo_GetDescript(CitizenBuff.RefInfo __instance, ref string __result)
        {
            if (!ActiveCustomSpecialUnit || !TryGetCustomCharInfo(__instance.RefName, out CustomCharInfo charInfo))
                return true;

            __result = charInfo.description;

            return false;
        }

        #endregion

        #endregion

        #region 全正面特征

        /// <summary>
        /// 生成角色特性
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        public static bool CCMake_Info_MakeCharacterList(CCMake_Info __instance)
        {
            return MakeCharacterList(__instance);
        }

        /// <summary>
        /// 生成特性列表
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        static bool MakeCharacterList(CCMake_Info info)
        {
            if (info.List_CharInfo == null)
                info.List_CharInfo = new List<int>();

            if (ActiveOnlyGoodCharacteristic)
            {
                info.List_CharInfo.Add(GoodCharDB_1[RandomInt(0, GoodCharDB_1.Count)].Index);

                info.List_CharInfo.Add(GoodCharDB_2[RandomInt(0, GoodCharDB_2.Count)].Index);

                return false;
            }

            info.List_CharInfo.Add(DBMgr.m_CharacterDB.List_Char1_DB[RandomInt(0, DefaultChar1Count)].Index);

            info.List_CharInfo.Add(DBMgr.m_CharacterDB.List_Char2_DB[RandomInt(0, DefaultChar2Count)].Index);

            return false;
        }

        #endregion

        #region 更多负重

        /// <summary>
        /// 获取负重
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="dmg"></param>
        public static bool GameUnit_Get_HandCapacity(GameUnit __instance, ref int __result)
        {
            if (!ActiveMoreCapacityByPower)
                return true;

            __result = (int)__instance.m_TP_Value - (int)(__instance.GetPDI(PDI.Power) * 0.5f) + __instance.GetPDI(PDI.Power);

            if (__result < 2)
                __result = 2;

            return false;
        }

        #endregion

        #region 更多经验

        static float? m_EXP_Value = null;
        /// <summary>
        /// 更新属性前
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="dmg"></param>
        public unsafe static void GameUnit_UpdatePDI(GameUnit __instance)
        {
            if (!ActiveMoreExp)
                return;

            m_EXP_Value = __instance.m_EXP_Value;

            __instance.m_EXP_Value += 0.5f;

            //Debug.Log($"当前经验增幅 {m_EXP_Value} -> {__instance.m_EXP_Value}");
        }

        /// <summary>
        /// 更新属性后
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="dmg"></param>
        public static void GameUnit_UpdatePDI_Post(GameUnit __instance, PDI _pdi, float value)
        {
            //Debug.Log($"获得了 {_pdi} 经验 {value}");

            if (m_EXP_Value == null)
                return;

            __instance.m_EXP_Value = (float)m_EXP_Value;

            m_EXP_Value = null;
        }

        #endregion

        #region 猎人受到的伤害减半

        /// <summary>
        /// 单位受击时
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="dmg"></param>
        public static void GameUnit_BeAttacked(GameUnit __instance, ref float dmg)
        {
            if (!ActiveHunterBeAttackGetHalfDamage || __instance.m_Job.m_Info.Ability != BuildAbility.HunterHut)
                return;

            dmg *= 0.5f;
        }

        #endregion

        #region 连续拾取

        /// <summary>
        /// 选中的地图格
        /// </summary>
        static QueenCheckBox selectedOutQueenCheckBox = null;
        /// <summary>
        /// 选中的物品类型
        /// </summary>
        static TileType selectedType;
        /// <summary>
        /// 拾起物品
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="dmg"></param>
        public static void QueenCheckBox_GatherSelected(QueenCheckBox __instance)
        {
            if (!ActiveContinuousTakeOutItems || __instance == null || __instance.m_SelectNum < 0 || __instance.m_SelectNum >= __instance.List_MiniInfo.Count)
                return;

            selectedOutQueenCheckBox = __instance;

            selectedType = __instance.List_MiniInfo[__instance.m_SelectNum].List_TileObj[0].m_Info.m_Type;

            //Debug.Log($"拾起物品，当前选中 {__instance.m_SelectNum}/{__instance.Obj_InfoBox.Length}");
        }

        #endregion

        #region 默认选中已拾起的物品

        static T_Queen queen;
        /// <summary>
        /// 触发目标格
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="dmg"></param>
        public static void QueenCheckBox_OnTriggerEnter2D(QueenCheckBox __instance, Collider2D collision)
        {
            if (!ActiveDefaultSelectSameItem || __instance.List_MiniInfo.Count == 0)
                return;

            //Debug.Log($"触发物品，{__instance.List_MiniInfo.Count}/{__instance.Obj_InfoBox.Length}");

            queen = GetPrivateValue<T_Queen>(__instance, "m_Queen");

            if (queen.List_Gathering.Count == 0)
                return;

            TileObject tObj = collision.GetComponent<TileObject>();

            MiniInfo miniInfo;

            int index = -1;

            for (int i = 0; i < __instance.List_MiniInfo.Count; i++)
            {
                miniInfo = __instance.List_MiniInfo[i];

                if (miniInfo.m_Type != MiniType.TileObj || miniInfo.List_TileObj.Count == 0 || miniInfo.List_TileObj[0].m_Info.m_Type != queen.List_Gathering[0].m_Type)
                    continue;

                index = i;

                Debug.Log($"{tObj} {queen.List_Gathering[0].m_Type}，目标 [{index}]{miniInfo.List_TileObj[0].m_Info.m_Type}");
            }

            if (index == -1)
                return;

            __instance.m_SelectNum = index;

            //Debug.Log($"{queen.m_CharState} 当前拾取了 {queen.List_Gathering[0].m_Type}，选中了 [{index}/{__instance.m_SelectNum}]{__instance.List_MiniInfo[index].List_TileObj[0].m_Info.m_Type}");
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(QueenCheckBox), "BoxListUpdate")]
        //public static void QueenCheckBox_BoxListUpdate(QueenCheckBox __instance, ref int list_index)
        //{
        //    Debug.Log($"更新下标 {list_index} {__instance.m_OnNum} / {__instance.m_SelectNum}");
        //}

        #endregion

        #region 建造无需材料到位

        //static bool prevSafeStoreExist;
        ///// <summary>
        ///// 建造合法性验证
        ///// </summary>
        ///// <param name="__instance"></param>
        //[HarmonyPrefix, HarmonyPatch(typeof(MiningBox), "BuildEnableCheck")]
        //public static void MiningBox_BuildEnableCheck(MiningBox __instance)
        //{
        //    if (!ActiveBluePrintNoNeedRes)
        //        return;

        //    prevSafeStoreExist = BuildingMgr.m_SafeStoreExist;

        //    BuildingMgr.m_SafeStoreExist = false;
        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(MiningBox), "BuildEnableCheck")]
        //public static void MiningBox_BuildEnableCheck_Post()
        //{
        //    if (!ActiveBluePrintNoNeedRes)
        //        return;

        //    BuildingMgr.m_SafeStoreExist = prevSafeStoreExist;
        //}

        //[HarmonyPrefix, HarmonyPatch(typeof(BuildingMgr), "IsMatEnough", new Type[] { typeof(Building), typeof(List<TileType>), typeof(List<int>) })]
        //public static bool BuildingMgr_IsMatEnough(Building _building, ref bool __result)
        //{
        //    if (!ActiveBluePrintNoNeedRes)
        //        return true;

        //    //Debug.Log($"建造目标 {_building.m_Info.T_Name}");

        //    __result = true;

        //    return false;
        //}

        ///// <summary>
        ///// 建筑的材料槽是否有足够的材料
        ///// </summary>
        ///// <param name="_building"></param>
        ///// <param name="__result"></param>
        ///// <returns></returns>
        //[HarmonyPrefix, HarmonyPatch(typeof(BuildMid_NeedMatSlot), "IsSatisfy")]
        //public static bool BuildMid_NeedMatSlot_IsSatisfy(ref bool __result)
        //{
        //    if (!ActiveBluePrintNoNeedRes)
        //        return true;

        //    __result = true;

        //    return false;
        //}

        #endregion

        #region 友方掉落无伤、神医在世、量子机械

        /// <summary>
        /// 市民受到伤害
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_tag"></param>
        /// <returns></returns>
        public static bool T_Citizen_BeAttacked(T_Citizen __instance, ref float dmg, Unit_Attacekd_Tag _tag)
        {
            //友方掉落无伤：受到掉落伤害且已启用掉落无伤时
            if (ActiveDropNoDamgeWithOurTeam && _tag == Unit_Attacekd_Tag.Falling)
                return false;

            float value = dmg;

            //启用了神医在世，且不在受伤状态下
            if (CustomCharInfoIsActive("HT_SYZS") && __instance.m_CharState != CharState.Injury)
            {
                float injuryThreshold = __instance.m_MaxHP * 0.2f;

                //最大伤害为可致受伤
                float dmgLimit = __instance.m_CurHP > injuryThreshold ? __instance.m_CurHP - injuryThreshold : 0;

                dmg = dmg < -dmgLimit ? -dmgLimit : dmg;
            }

            //奥米伽不会受到致死伤害
            if (CitizenHaveCharacterInfo(__instance, "AMJ7_LZJX", out _))
                dmg = __instance.m_CurHP + dmg <= 0 ? 0 : dmg;

            //Debug.Log($"{__instance.m_UnitName} 受到 {value}->{dmg} 伤害，状态 {__instance.m_CharState}，来源 {_tag}，华佗在世 {CustomCharInfoIsUsed("HT_SYZS")}");

            return true;
        }

        /// <summary>
        /// 女王受到伤害
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_tag"></param>
        /// <returns></returns>
        public static bool T_Queen_BeAttacked(T_Queen __instance, Unit_Attacekd_Tag _tag)
        {
            return !ActiveDropNoDamgeWithOurTeam || _tag != Unit_Attacekd_Tag.Falling;
        }

        #endregion

        #region 共享床位

        /// <summary>
        /// 基类方法
        /// </summary>
        /// <param name="instance"></param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void BaseBuildingWorkingStop(Building_House instance) { }

        /// <summary>
        /// 床位停止工作
        /// </summary>
        /// <param name="__instance"></param>
        public static bool Building_House_BuildingWorkingStop(Building_House __instance)
        {
            if (!ActiveShareHome)
                return true;

            BaseBuildingWorkingStop(__instance);

            for (int i = 0; i < __instance.Arr_Master.Length; i++)
            {
                if (__instance.Arr_Master[i] != null && __instance.Arr_Master[i].m_ImFatigue == 2)
                {
                    __instance.Arr_Master[i].UnSleep();

                    //__instance.Arr_Master[i].BehaviorStop(true);
                }
            }

            return false;
        }

        /// <summary>
        /// 市民结束睡觉
        /// </summary>
        /// <param name="__instance"></param>
        public static void T_Citizen_UnSleep(T_Citizen __instance)
        {
            if (!ActiveShareHome || __instance.m_Home == null || __instance.m_Home.m_HouseBatchInfo.m_Option != HouseOption.AutoBatch)
                return;

            __instance.m_Home.RemoveMaster(__instance);

            if (__instance.m_SpeechBubble != null)
                __instance.m_SpeechBubble.SpeechBubbleOff();

            __instance.SetAniState(AniState.Idle, true, true);

            __instance.Update_CharAbility(113, false);

            //Debug.Log($"{__instance.m_UnitName} 离开了共享床位，当前床位 {__instance.m_Home.Arr_Master}");
        }

        #endregion

        #region 贸易消息

        /// <summary>
        /// 贸易完成事件
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="result"></param>
        /// <param name="__result"></param>
        public static void DiplomaticMgr_OnTradeResultEvent(DiplomaticMgr __instance, TradeResult result, TradeReceive __result)
        {
            //目标城市名称
            string targetName = result.Sheet.CountryData.Name;

            string countyComValue = CountryCommercialityDatas.TryGetValue(result.Sheet.CountryData.Name, out float comValue) ? $"[{comValue}/1000]" : "";

            //资源信息
            string resInfo = string.Format("{0} * {1}", DBMgr.Dic_TileDB.TryGetValue(result.Sheet.Resource, out TileInfo info) ? info.T_Name : result.Sheet.Resource.ToString(), result.Sheet.ResourceData.CountPerPackage * result.Sheet.Count);

            string msg = result.Sheet.TypeTrade == TypeTrade.Country_To_Hometown ? $"从 {targetName}{countyComValue} 运来 {resInfo}" : $"向 {targetName}{countyComValue} 售卖 {resInfo}";

            string resultInfo = "";

            switch (__result.TradeReceiveState)
            {
                //成功
                case TradeReceiveState.Success: resultInfo = ", 交易成功"; break;
                //资源错误
                case TradeReceiveState.Resource_Fail: resultInfo = ", 资源不足，出售失败"; break;
                //货币错误
                case TradeReceiveState.Asset_Fail: resultInfo = ", 货币不足，购买失败"; break;
                //仓库错误
                case TradeReceiveState.Storage_Fail:

                    if (result.Sheet.BuildingID == null)
                    {
                        resultInfo = ", 仓库编号错误，购买失败";

                        break;
                    }

                    Building_Storage storage = (Building_Storage)BuildingMgr.List_Building.Find((Building b) => b.m_ID == result.Sheet.BuildingID.Value);

                    if (storage == null)
                    {
                        resultInfo = ", 仓库不存在，购买失败";

                        break;
                    }

                    resultInfo = ", 仓库状态异常，购买失败";

                    break;
            }

            Debug.Log($"贸易完成，{msg}{resultInfo}");

            if (ActiveTradeResultMessage)
                CenterMessage($"{msg}{resultInfo}");
        }

        #endregion

        #region 贸易详细信息

        static string perCountText = "束";
        /// <summary>
        /// 设置订单槽界面数据设置
        /// </summary>
        /// <param name="cData"></param>
        /// <param name="sData"></param>
        /// <param name="____typeTradeSheet"></param>
        /// <param name="____upperText"></param>
        /// <param name="____lowerText"></param>
        public static void DiplomaticTradeSheetSlotUI_SetData(DiplomaticCountryData cData, DiplomaticCountryTradeSheetData sData, TypeTradeSheet ____typeTradeSheet, TextMeshProUGUI ____upperText, TextMeshProUGUI ____lowerText)
        {
            if (!ActiveSheetMoreInfo || ____typeTradeSheet != TypeTradeSheet.Count)
                return;

            DiplomaticCountryResourceData resourceData = cData.GetResourceDataOrNull(sData.Resource);

            int count = sData.Count;

            ____upperText.text = $"{resourceData.CountPerPackage * count}个 ({count}{perCountText})";

            float price = sData.TypeTrade == TypeTrade.Hometown_To_Country ? sData.PackageTradeHometownToCountryPrice(true) : sData.PackageTradeCountryToHometownPrice(true);

            ____lowerText.text = $"{UIUtility.GetTranslate("Word/Dip trade price")} {cData.UseMoneyIcon} {ColorText_F}{price}{ColorText_B} * {count}{perCountText} = {cData.UseMoneyIcon}{ColorText_F}{price * count}{ColorText_B}";
        }

        /// <summary>
        /// 设置订单槽详细（左）界面数据设置
        /// </summary>
        /// <param name="cData"></param>
        /// <param name="sData"></param>
        /// <param name="____typeTradeSheet"></param>
        /// <param name="____upperText"></param>
        /// <param name="____lowerText"></param>
        public static void DiplomaticTradeSheetDetailSlotUI_Refresh(TextMeshProUGUI ____text, int ____nowValue, DiplomaticCountryTradeSheetData ____sData, TypeTradeSheet ____typeTradeSheet)
        {
            if (!ActiveSheetMoreInfo || ____typeTradeSheet != TypeTradeSheet.Count)
                return;

            ____text.text = $"{____sData.ResourceData.CountPerPackage * ____nowValue}个 ({____nowValue}{perCountText})";
        }

        /// <summary>
        /// 贸易详细内容界面（左）数据设置
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="cData"></param>
        /// <param name="sData"></param>
        /// <param name="typeTradeSheet"></param>
        /// <param name="____infoList"></param>
        public static void DiplomaticTradeSheetDetailContentsUI_SetData(DiplomaticTradeSheetDetailContentsUI __instance, DiplomaticCountryData cData, DiplomaticCountryTradeSheetData sData, TypeTradeSheet typeTradeSheet, List<DiplomaticTradeSheetDetailInfoUI> ____infoList)
        {
            if (!ActiveCustomSpecialUnit || typeTradeSheet != TypeTradeSheet.Resource)
                return;

            DiplomaticTradeSheetDetailInfoUI infoUI = ____infoList[____infoList.Count - 1];

            float value = sData.TypeTrade == TypeTrade.Hometown_To_Country ? sData.PackageTradeHometownToCountryPrice(true) : sData.PackageTradeCountryToHometownPrice(true);

            infoUI.SetValue($"{ColorText_F}{value}{SymbolText}{ColorText_B}");
        }

        ///// <summary>
        ///// 设置订单槽详细（左）界面数据设置
        ///// </summary>
        ///// <param name="cData"></param>
        ///// <param name="sData"></param>
        ///// <param name="____typeTradeSheet"></param>
        ///// <param name="____upperText"></param>
        ///// <param name="____lowerText"></param>
        //[HarmonyPostfix, HarmonyPatch(typeof(DiplomaticTradeSheetDetailSlotUI), "SetData", new Type[] { typeof(DiplomaticCountryData), typeof(DiplomaticCountryTradeSheetData), typeof(TypeTradeSheet) })]
        //public static void DiplomaticTradeSheetDetailSlotUI_SetData(DiplomaticCountryData cData, DiplomaticCountryTradeSheetData sData, TypeTradeSheet typeTradeSheet, ref int ____maxValue)
        //{
        //    if (typeTradeSheet != TypeTradeSheet.Count)
        //        return;

        //    ____maxValue = cData.NowProsperity + 1;
        //}

        #endregion

        #region 乌托邦模式：所有物品价格、服务费用、工作工资为0（除搬运、挖掘等基础工作），所有建筑、物品无阶级限制，可同时兼容法律设置

        static List<TileInfo> DefaultTileInfo = new List<TileInfo>();
        static List<BuildInfo> DefaultBuildInfo = new List<BuildInfo>();

        /// <summary>
        /// 加载初始物品信息
        /// </summary>
        static void LoadDefaultTileInfo()
        {
            DefaultTileInfo.Clear();

            foreach (KeyValuePair<TileType, TileInfo> keyValue in DBMgr.Dic_TileDB)
            {
                TileInfo info = new TileInfo();

                info.Price = keyValue.Value.Price;

                info.Level = keyValue.Value.Level;

                DefaultTileInfo.Add(info);
            }
        }

        /// <summary>
        /// 加载初始建筑信息
        /// </summary>
        static void LoadDefaultBuildInfo()
        {
            DefaultBuildInfo.Clear();

            foreach (KeyValuePair<BuildingName, BuildInfo> keyValue in DBMgr.Dic_BuildDB)
            {
                BuildInfo info = new BuildInfo();

                info.Cost = keyValue.Value.Cost;

                info.Payment = keyValue.Value.Payment;

                info.Level = keyValue.Value.Level;

                DefaultBuildInfo.Add(info);
            }
        }

        /// <summary>
        /// 更新物品数据为乌托邦模式
        /// </summary>
        /// <param name="utopiaMode"></param>
        public static void UpdateTileDBToUtopiaMode(bool utopiaMode)
        {
            int i = 0;

            foreach (KeyValuePair<TileType, TileInfo> keyValue in DBMgr.Dic_TileDB)
            {
                if (i >= DefaultTileInfo.Count)
                {
                    Debug.LogError("乌托邦物品数据异常");

                    return;
                }

                keyValue.Value.Price = !utopiaMode ? DefaultTileInfo[i].Price : 0;

                keyValue.Value.OriginPrice = keyValue.Value.Price;

                keyValue.Value.Level = !utopiaMode ? DefaultTileInfo[i].Level : 0;

                keyValue.Value.OriginLevel = keyValue.Value.Level;

                i++;
            }

            Debug.Log($"共重载了 {i}/{DBMgr.Dic_TileDB.Count} 物品");
        }

        /// <summary>
        /// 更新建筑数据为乌托邦模式
        /// </summary>
        /// <param name="utopiaMode"></param>
        public static void UpdateBuildDBToUtopiaMode(bool utopiaMode)
        {
            int i = 0;

            foreach (KeyValuePair<BuildingName, BuildInfo> keyValue in DBMgr.Dic_BuildDB)
            {
                if (i >= DefaultBuildInfo.Count)
                {
                    Debug.LogError("乌托邦模式建筑数据异常");

                    return;
                }

                //建造费用
                keyValue.Value.Cost = !utopiaMode ? DefaultBuildInfo[i].Cost : 0;

                //工资
                keyValue.Value.Payment = !utopiaMode ? DefaultBuildInfo[i].Payment : 0;

                //权限等级
                keyValue.Value.Level = !utopiaMode ? DefaultBuildInfo[i].Level : 0;

                //初始权限等级
                keyValue.Value.OriginLevel = keyValue.Value.Level;

                i++;
            }

            Debug.Log($"共重载了 {i}/{DBMgr.Dic_BuildDB.Count} 建筑");
        }

        /// <summary>
        /// 获得建筑价值
        /// </summary>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool Building_GetBuildingValue(Building __instance, ref float __result)
        {
            if (!ActiveUtopiaMode || __instance.m_Info.Enable == 2)
                return true;

            int price;

            __result = 0;

            for (int i = 0; i < __instance.m_Info.List_Material.Count; i++)
            {
                if (!TileOriginePriceDatas.TryGetValue(__instance.m_Info.List_Material[i], out price))
                    continue;

                __result += price * __instance.m_Info.List_Material_Num[i];
            }

            return false;
        }

        /// <summary>
        /// 服务费用
        /// </summary>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool Commercial_SubPannel_GetServicePrice(ref float __result)
        {
            if (!ActiveUtopiaMode)
                return true;

            __result = 0f;

            return false;
        }

        /// <summary>
        /// 获得当前价格
        /// </summary>
        /// <param name="____originValue"></param>
        /// <param name="____resource"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool DiplomaticCountryResourceData_OriginValue(DiplomaticCountryResourceData __instance, ref float ____nowValue, ref float __result)
        {
            if (____nowValue <= 0f)
                __instance.ChangeNowValue();

            __result = ____nowValue;

            return false;
        }

        /// <summary>
        /// 变更当前价格
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="____nowValue"></param>
        public static void DiplomaticCountryResourceData_ChangeNowValue(DiplomaticCountryResourceData __instance, ref float ____nowValue)
        {
            if (____nowValue <= 0)
                ____nowValue = __instance.OriginValue;
        }

        /// <summary>
        /// 获得原始价格
        /// </summary>
        /// <param name="____originValue"></param>
        /// <param name="____resource"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool DiplomaticCountryResourceData_OriginValue(ref float ____originValue, TileType ____resource, ref float __result)
        {
            if (____originValue <= 0f)
            {
                ____originValue = TileOriginePriceDatas.TryGetValue(____resource, out int price) ? price : 10f;

                //Debug.Log($"更新 {____resource} 的基础价格为 {____originValue}");
            }

            __result = ____originValue;

            //Debug.Log($"{____resource} 基础价格 {__result}");

            return false;
        }

        #endregion

        #region 和平模式

        /// <summary>
        /// 黄鼠狼帐篷进攻
        /// </summary>
        /// <returns></returns>
        public static bool WeaseTent_WaveCall()
        {
            return !ActiveSafeMode;
        }

        /// <summary>
        /// 僵尸洞进攻
        /// </summary>
        /// <returns></returns>
        public static bool ZombieHole_WaveCall()
        {
            return !ActiveSafeMode;
        }

        /// <summary>
        /// 黄鼠狼帐篷受到攻击
        /// </summary>
        /// <returns></returns>
        public static bool WeaselTent_BeAttacked(WeaselTent __instance, float dmg)
        {
            if (!ActiveSafeMode)
                return true;

            //((MapObj)__instance).BeAttacked(dmg);

            return false;
        }

        /// <summary>
        /// 僵尸洞受到攻击
        /// </summary>
        /// <returns></returns>
        public static bool ZombieHole_BeAttacked(ZombieHole __instance, float dmg)
        {
            if (!ActiveSafeMode)
                return true;

            //((MapObj)__instance).BeAttacked(dmg);

            return false;
        }

        ///// <summary>
        ///// 黄鼠狼帐篷生成
        ///// </summary>
        ///// <param name="__instance"></param>
        ///// <param name="dmg"></param>
        ///// <returns></returns>
        //[HarmonyPrefix, HarmonyPatch(typeof(WeaselTent), "MakeMapObj")]
        //public static bool WeaselTent_MakeMapObj()
        //{
        //    return PlayDataMgr.Instance.IsLoadGame || !ActiveSafeMode;
        //}

        ///// <summary>
        ///// 僵尸洞生成
        ///// </summary>
        ///// <returns></returns>
        //[HarmonyPrefix, HarmonyPatch(typeof(ZombieHole), "MakeMapObj")]
        //public static bool ZombieHole_MakeMapObj()
        //{
        //    return PlayDataMgr.Instance.IsLoadGame || !ActiveSafeMode;
        //}

        #endregion

        #region 敌方死亡掉落

        /// <summary>
        /// 敌方死亡检测
        /// </summary>
        /// <param name="__instance"></param>
        public static void GameEnemy_DeathCheck(GameEnemy __instance)
        {
            if (!ActiveEnemyDeadthDrop)
                return;

            RunEnemyDrop(__instance);
        }

        /// <summary>
        /// 精英敌方死亡
        /// </summary>
        /// <param name="__instance"></param>
        public static void EliteEnemy_DeathCheck(EliteEnemy __instance)
        {
            if (!ActiveEnemyDeadthDrop)
                return;

            RunEnemyDrop(__instance);
        }

        /// <summary>
        /// 执行敌方掉落
        /// </summary>
        /// <param name="enemy"></param>
        static void RunEnemyDrop(GameEnemy enemy)
        {
            if (!TryGetEnemyDropData(enemy.m_EnemyInfo.m_EnemyName, out CustomEnemyDrop enemyDrop) || enemyDrop.dropList.Count == 0)
                return;

            Vector3 pos = enemy.GetPos();

            int count = 0;

            foreach (TileDrop drop in enemyDrop.dropList)
            {
                while (count < drop.count && IntProbability <= drop.proValue)
                {
                    count++;
                }

                CreateTileObj(drop.name, pos, count);
            }
        }

        /// <summary>
        /// 尝试获取敌方掉落
        /// </summary>
        /// <param name="name"></param>
        /// <param name="drop"></param>
        /// <returns></returns>
        static bool TryGetEnemyDropData(EnemyType name, out CustomEnemyDrop drop)
        {
            return EnemyDropDatas.TryGetValue(name, out drop);
        }

        #endregion

        #region 无限人口

        /// <summary>
        /// 更新人口文本
        /// </summary>
        /// <param name="__instance"></param>
        public static bool CitizenUI_CitizenTxtUpdate()
        {
            UpdateCitizenTxt();

            return false;
        }

        /// <summary>
        /// 更新市民人数文本
        /// </summary>
        /// <returns></returns>
        static void UpdateCitizenTxt()
        {
            int count = Citizens.Count;

            EcoMgr.m_CitizenUI.Txt_Num.text = ActivePoPUnLimit ? count.ToString() : $"{count} / {ProsperityUI.m_CurPros.Pop}";

            //Debug.Log($"{ActivePoPUnLimit} {ActiveAddPopLimit} {EcoMgr.m_CitizenUI.Txt_Num.text}");

            AchievementManager.Instance.SetValue(TypeAchievementCategory.ImmigrantCount, "val", count);
        }

        #endregion

        #region 移除输入框字符长度限制

        /// <summary>
        /// 显示输入框
        /// </summary>
        /// <param name="__instance"></param>
        public static void InputtableManager_Show(ref int length)
        {
            if (!ActiveNameLengthUnLimit)
                return;

            length = 0;
        }

        #endregion

        #region 显示单位移动路径

        /// <summary>
        /// 绘制路径
        /// </summary>
        public static void DrawWay(bool value)
        {
            SystemMgr.m_ShowUpCitizenLine = !value;

            SystemMgr.DrawWayCall();
        }

        #endregion

        #region 地图编辑模式

        /// <summary>
        /// 绘制路径
        /// </summary>
        public static void PallateMode(bool value)
        {
            TileMgr.IsSandBoxMode = value;

            if (!value)
            {
                //GameMgr.Instance._CamMgr.ZoomSizeUpdate(6.2f);

                for (int i = 0; i < PallateMgr.m_Icons.Length; i++)
                {
                    if (PallateMgr.m_Icons[i].m_Outline.enabled)
                    {
                        PallateMgr.m_Icons[i].MouseUp();
                    }
                }

                PallateMgr.m_BrushType = 0;
            }

            PallateMgr.Obj_Main.SetActive(value);
        }

        #endregion

        #region 共享仓库

        /// <summary>
        /// 建造仓库
        /// </summary>
        /// <param name="__instance"></param>
        public static void Building_Storage_BuildingSet(Building_Storage __instance)
        {
            if (!ActiveShareStorage || __instance.m_Info.Ability != BuildAbility.Store || StorageCount == 0)
                return;

            __instance.List_TileObj = BuildingMgr.List_Storage[0].List_TileObj;

            Debug.Log($"建造了仓库 [{StorageCount - 1}]{__instance.m_CustomName}（{__instance.List_TileObj.Count}物品<={BuildingMgr.List_Storage[0].m_CustomName}） 当前共有仓库 {StorageCount}");
        }

        /// <summary>
        /// 拆毁仓库
        /// </summary>
        /// <param name="__instance"></param>
        public static void Building_Storage_BuildingDemolition(Building_Storage __instance)
        {
            if (!ActiveShareStorage || __instance.m_Info.Ability != BuildAbility.Store || StorageCount < 2)
                return;

            __instance.List_TileObj = new List<TileSt_Info>();
        }

        /// <summary>
        /// 获取食物数量
        /// </summary>
        /// <param name="__instance"></param>
        public static bool BuildingMgr_GetAllFood(BuildingMgr __instance, ref int __result)
        {
            if (!ActiveShareStorage || StorageCount == 0)
                return true;

            Building_Storage storage = __instance.List_Storage[0];

            __result = 0;

            TileSt_Info info;

            List<TileInfo> foodTiles = DBMgr.Dic_TileDB.Values.Where(t => t.Category == ResCateogry.Food).ToList();

            for (int i = 0; i < storage.List_TileObj.Count; i++)
            {
                info = storage.List_TileObj[i];

                if (foodTiles.Find(t => t.m_TileType == info.m_Type) == null)
                    continue;

                __result += info.List_Reservation.Count;
            }

            return false;
        }

        /// <summary>
        /// 获取仓库里的材料数量
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_type"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool BuildingMgr_GetStorageMatNum(BuildingMgr __instance, TileType _type, ref int __result)
        {
            if (!ActiveShareStorage || StorageCount < 2)
                return true;

            TileSt_Info stInfo = __instance.List_Storage[0].List_TileObj.Find(t => t.m_Type == _type);

            __result = stInfo != null ? stInfo.List_Reservation.Count : 0;

            return false;
        }

        /// <summary>
        /// 查找仓库的材料数量
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_type"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool BuildingMgr_Find_ResourceNum(BuildingMgr __instance, TileType _type, ref int __result)
        {
            if (!ActiveShareStorage || StorageCount < 2)
                return true;

            TileSt_Info stInfo = __instance.List_Storage[0].List_TileObj.Find(t => t.m_Type == _type);

            __result = stInfo != null ? stInfo.List_Reservation.Count : 0;

            return false;
        }

        /// <summary>
        /// 更新仓库界面信息
        /// </summary>
        /// <param name="__instance"></param>
        public static void BI_StorageUI_InfoUpdate(BI_StorageUI __instance, TextMeshProUGUI ___Txt_Num)
        {
            if (!ActiveShareStorage)
                return;

            //只显示当前物品种类数量
            ___Txt_Num.text = __instance.m_Building.List_TileObj.Count.ToString();
        }

        /// <summary>
        /// 绑定所有仓库的物品
        /// </summary>
        /// <param name="list"></param>
        static void BindAllStorageTileList(List<TileSt_Info> list)
        {
            foreach (Building_Storage storage in BuildingMgr.List_Storage)
            {
                storage.List_TileObj = list;

                storage.m_BuildInfoUI.InfoUpdate();
            }
        }

        /// <summary>
        /// 更新仓库容量
        /// </summary>
        /// <param name="value"></param>
        static void UpdateStorageCapacity(bool value)
        {
            Building_Storage storage;

            //更新当前所有仓库的容量
            for (int i = 0; i < BuildingMgr.List_Storage.Count; i++)
            {
                storage = BuildingMgr.List_Storage[i];

                if (StorageCapacityDatas.TryGetValue(storage.m_Info.Name, out int capacity))
                    storage.m_Info.EffectValue1_Num = value ? 9999 : capacity * (ActiveAddStorageCapacity ? 2 : 1);
            }

            //更新数据的容量
            foreach (KeyValuePair<BuildingName, BuildInfo> keyValue in StorageDB)
            {
                if (StorageCapacityDatas.TryGetValue(keyValue.Value.Name, out int capacity))
                    keyValue.Value.EffectValue1_Num = value ? 9999 : capacity * (ActiveAddStorageCapacity ? 2 : 1);
            }
        }

        #endregion

        #region 仓库直供

        /// <summary>
        /// 节点建筑
        /// </summary>
        static Dictionary<int, Building> NodeBuildings = new Dictionary<int, Building>();
        /// <summary>
        /// 拥有直供仓库的建筑
        /// </summary>
        static Dictionary<int, Building_Storage[]> HaveDirectSupplyBuildings = new Dictionary<int, Building_Storage[]>();
        /// <summary>
        /// 建筑直供材料的花费
        /// </summary>
        static Dictionary<int, int> BuildingMaterialsCost = new Dictionary<int, int>();

        /// <summary>
        /// 直供参数清理
        /// </summary>
        static void DirectSupplyClear()
        {
            NodeBuildings.Clear();

            HaveDirectSupplyBuildings.Clear();

            BuildingMaterialsCost.Clear();
        }

        /// <summary>
        /// 建筑建造后
        /// </summary>
        /// <param name="__instance"></param>
        public static void Building_BuildingSet_Post(Building __instance)
        {
            foreach (Vector2Int pos in __instance.List_BuildPos)
            {
                NodeBuildings[TileMgr.GetNode(pos).m_Key] = __instance;
            }

            bool isStorage = __instance is Building_Storage;

            //设置左侧相邻建筑
            SetNearbyBuilding(__instance, __instance.List_GroundPos, true, isStorage);

            //设置右侧相邻建筑
            SetNearbyBuilding(__instance, __instance.List_GroundPos, false, isStorage);
        }

        /// <summary>
        /// 建筑拆除后
        /// </summary>
        /// <param name="__instance"></param>
        public static void Building_BuildingDemolition(Building __instance)
        {
            foreach (Vector2Int pos in __instance.List_BuildPos)
            {
                int key = TileMgr.GetNode(pos).m_Key;

                if (NodeBuildings.ContainsKey(key))
                    NodeBuildings.Remove(key);
            }

            bool isStorage = __instance is Building_Storage;

            //自身非仓库且在直供建筑中时移除
            if (!isStorage)
            {
                int key = __instance.m_ID;

                if (HaveDirectSupplyBuildings.ContainsKey(key))
                    HaveDirectSupplyBuildings.Remove(key);

                if (BuildingMaterialsCost.ContainsKey(key))
                    BuildingMaterialsCost.Remove(key);
            }
            //自身为仓库
            else
            {
                //左侧非仓库时，清空左侧建筑的右仓库信息
                if (TryGetNearbyBuilding(__instance.List_GroundPos, true, out Building nearbyBuilding) && !(nearbyBuilding is Building_Storage) &&
                    HaveDirectSupplyBuildings.TryGetValue(nearbyBuilding.m_ID, out Building_Storage[] storages))
                    storages[1] = null;

                //右侧非仓库时，清空右侧建筑的左仓库信息
                if (TryGetNearbyBuilding(__instance.List_GroundPos, false, out nearbyBuilding) && !(nearbyBuilding is Building_Storage) &&
                    HaveDirectSupplyBuildings.TryGetValue(nearbyBuilding.m_ID, out storages))
                    storages[0] = null;
            }
        }

        /// <summary>
        /// 建筑启用状态检测
        /// </summary>
        /// <param name="__instance"></param>
        public static void Building_ActivateCheck(Building __instance)
        {
            if (!ActiveDirectSupplyStorage)
                return;

            AddMaterialsFromNearStorage(__instance.m_BuildInfoUI as MasonryInfo);
        }

        /// <summary>
        /// 工作监听中
        /// </summary>
        static bool WorkUpdating = false;
        /// <summary>
        /// 建筑工作监听前
        /// </summary>
        /// <param name="__instance"></param>
        public static void MasonryInfo_WorkUpdate(MasonryInfo __instance)
        {
            if (!ActiveDirectSupplyStorage)
                return;

            WorkUpdating = true;

            AddMaterialsFromNearStorage(__instance);
        }

        /// <summary>
        /// 建筑工作监听后
        /// </summary>
        /// <param name="__instance"></param>
        public static void MasonryInfo_WorkUpdate_Post(MasonryInfo __instance)
        {
            WorkUpdating = false;
        }

        /// <summary>
        /// 设置个人统计数据（在m_Building.AddValue添加物品后）
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="key"></param>
        /// <param name="res"></param>
        /// <param name="value"></param>
        public static void MasonryInfo_SetIndividualStatisticsProduct(MasonryInfo __instance, int key, TileType res, int value)
        {
            if (!ActiveDirectSupplyStorage || !WorkUpdating)
                return;

            if (res == TileType.Coin || res == TileType.Money)
                return;

            if (AddItemToNearStorage(key, res, value))
                AddMaterialsFromNearStorage(__instance);
        }

        /// <summary>
        /// 设置相邻建筑
        /// </summary>
        /// <param name="building"></param>
        /// <param name="groundPos"></param>
        /// <param name="isLeft"></param>
        /// <param name="isStorage"></param>
        static void SetNearbyBuilding(Building building, List<Vector2Int> groundPos, bool isLeft, bool isStorage)
        {
            if (!TryGetNearbyBuilding(groundPos, isLeft, out Building nearbyBuilding))
                return;

            bool nearIsStorage = nearbyBuilding is Building_Storage;

            //若自身和相邻建筑同为仓库或同不为仓库
            if (isStorage == nearIsStorage)
                return;

            //自身是仓库时，目标变为相邻建筑
            int key = (isStorage ? nearbyBuilding : building).m_ID;

            //获取目标的相邻仓库信息
            if (!HaveDirectSupplyBuildings.TryGetValue(key, out Building_Storage[] storages))
            {
                storages = new Building_Storage[2];

                HaveDirectSupplyBuildings.Add(key, storages);

                BuildingMaterialsCost[key] = 0;
            }

            //自身是仓库时，则存储自身为相邻建筑的仓库，反之则存储相邻建筑为自身的仓库
            storages[isLeft ? 0 : 1] = (isStorage ? building : nearbyBuilding) as Building_Storage;
        }

        /// <summary>
        /// 尝试获取附近的建筑
        /// </summary>
        /// <param name="groundPos"></param>
        /// <param name="isLeft"></param>
        /// <param name="nearbyBuilding"></param>
        /// <returns></returns>
        static bool TryGetNearbyBuilding(List<Vector2Int> groundPos, bool isLeft, out Building nearbyBuilding)
        {
            nearbyBuilding = null;

            if (groundPos.Count == 0)
                return false;

            C_Node node = TileMgr.GetNode(new Vector2Int(isLeft ? groundPos[0].x - 1 : groundPos[groundPos.Count - 1].x + 1, groundPos[0].y + 1));

            return node != null && NodeBuildings.TryGetValue(node.m_Key, out nearbyBuilding);
        }

        /// <summary>
        /// 从周边仓库添加材料
        /// </summary>
        /// <param name="info"></param>
        static void AddMaterialsFromNearStorage(MasonryInfo info)
        {
            if (info == null)
                return;

            Building building = info.m_Building;

            //没有直供仓库的建筑
            if (!HaveDirectSupplyBuildings.TryGetValue(building.m_ID, out Building_Storage[] storages))
                return;

            //Debug.Log($"{building.m_CustomName} {building.m_ProductType} 物品库中存在 {info.m_ProductSlot.m_TileType} {DBMgr.Dic_TileDB.TryGetValue(info.m_ProductSlot.m_TileType, out _)}");

            //物品库中没有生产目标（如采集类建筑）
            if (!DBMgr.Dic_TileDB.TryGetValue(info.m_ProductSlot.m_TileType, out _))
                return;

            //材料消耗倍率
            int ratio = building.m_MassProduct ? 2 : 1;

            //制作目标
            TileInfo tileInfo = DBMgr.Dic_TileDB[info.m_ProductSlot.m_TileType];

            List<TileType> recipeMats = tileInfo.List_Material[info.m_RecipeNum];

            List<int> recipeMatCount = tileInfo.List_Material_Num[info.m_RecipeNum];

            int cost = 0;

            //迭代当前配方的材料列表
            for (int i = 0; i < recipeMats.Count; i++)
            {
                //当前材料类型
                TileType res = recipeMats[i];

                //当前材料所需数量
                int needAmount = recipeMatCount[i] * ratio;

                //建筑已有当前材料的数量
                int haveAmount = building.GetNum(res);

                //已有足够的该材料
                if (haveAmount >= needAmount)
                    continue;
                else
                    needAmount -= haveAmount;

                int price = DBMgr.Dic_TileDB[res].Price;

                Debug.Log($"{building.m_CustomName} 当前生产需要 {res.GetName()} x {needAmount}");

                //优先从左边仓库寻找
                foreach (Building_Storage storage in storages)
                {
                    if (storage == null)
                        continue;

                    if (!storage.IsExistType(res))
                    {
                        Debug.Log($"仓库 {storage.m_CustomName} 中没有材料 {res.GetName()}");

                        continue;
                    }

                    bool finish = TryRemoveItemFromStorage(storage, res, needAmount, out int removedAmount);

                    needAmount = needAmount - removedAmount > 0 ? needAmount : 0;

                    building.AddValue(res, TObjState.Basic, removedAmount);

                    cost += price * removedAmount;

                    Debug.Log($"建筑 {building.m_CustomName} 从仓库 {storage.m_CustomName} 直供获得了材料 {res.GetName()} x {removedAmount}，剩余需求数量 {needAmount}");

                    if (finish)
                        break;
                }
            }

            if (cost > 0 && BuildingMaterialsCost.ContainsKey(building.m_ID))
                BuildingMaterialsCost[building.m_ID] += cost;
        }

        /// <summary>
        /// 尝试从仓库移除物品
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="item"></param>
        /// <param name="requestAmount"></param>
        /// <param name="removedAmount "></param>
        /// <returns></returns>
        static bool TryRemoveItemFromStorage(Building_Storage storage, TileType item, int requestAmount, out int removedAmount)
        {
            removedAmount = Mathf.Min(storage.GetNum(item), requestAmount);

            storage.AddValue(item, TObjState.Basic, -removedAmount);

            return removedAmount >= requestAmount;
        }

        /// <summary>
        /// 添加物品进相邻仓库
        /// </summary>
        /// <param name="key"></param>
        /// <param name="res"></param>
        /// <param name="value"></param>
        static bool AddItemToNearStorage(int key, TileType res, int value)
        {
            if (!HaveDirectSupplyBuildings.TryGetValue(key, out Building_Storage[] storages))
                return false;

            bool added = false;

            //优先从左边仓库存取
            foreach (Building_Storage storage in storages)
            {
                if (storage == null)
                    continue;

                if (!storage.CanI_Add(res, value))
                {
                    Debug.LogWarning($"仓库 {storage.m_CustomName} 添加物品 {res.GetName()} x {value} 失败，当前容量 {storage.List_TileObj.Count} / {storage.m_Info.EffectValue1_Num}");

                    continue;
                }

                storage.AddValue(res, TObjState.Basic, value);

                added = true;
            }

            Building building = BuildingMgr.FindBuildingOrNull(key);

            int cost = BuildingMaterialsCost[key];

            int income = 0;

            if (added)
            {
                building.AddValue(res, TObjState.Basic, -value);

                income = DBMgr.Dic_TileDB[res].Price * value - cost;

                EcoMgr.PayForRat_Payment(building.m_Master, income);
            }
            else
                EcoMgr.BuyForRat_ForC_Resource(building.m_Master, cost, res);

            BuildingMaterialsCost[key] = 0;

            Debug.Log($"建筑 {building.m_CustomName} 生产了 {res.GetName()} x {value}，直接存入仓库 {added} 收入 {income}，直供材料共花费 {cost}，当前共有可直供的建筑 {HaveDirectSupplyBuildings.Count}");

            return added;
        }

        #endregion

        #region 更多名称

        static string[][] CustomNames_Female;
        static string[][] PerNames_Female
        {
            get
            {
                if (CustomNames_Female == null)
                {
                    CustomNames_Female = new string[2][];

                    CustomNames_Female[0] = CustomPerNames_Female_One;

                    CustomNames_Female[1] = CustomPerNames_Female_Two;
                }

                return CustomNames_Female;
            }
        }

        static string[][] CustomNames_Male;
        static string[][] PerNames_Male
        {
            get
            {
                if (CustomNames_Male == null)
                {
                    CustomNames_Male = new string[2][];

                    CustomNames_Male[0] = CustomPerNames_Male_One;

                    CustomNames_Male[1] = CustomPerNames_Male_Two;
                }

                return CustomNames_Male;
            }
        }

        /// <summary>
        /// 所有已有鼠鼠的名字
        /// </summary>
        static List<string> usedNames = new List<string>();
        /// <summary>
        /// 单次移民中所有用到的名字
        /// </summary>
        static List<string> tempUsedNames = new List<string>();

        /// <summary>
        /// 获得随机姓名
        /// </summary>
        /// <param name="_gender"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool CitizenCaveUI_GetRandomName(Gender _gender, ref string __result)
        {
            if (!ActiveCustomNames)
                return true;

            string name;

            do
            {
                string surName = CustomSurNames[RandomInt(0, CustomSurNames.Length)];

                int index = RandomInt(0, 2);

                string[] perNames = _gender == Gender.Female ? PerNames_Female[index] : PerNames_Male[index];

                name = $"{surName}{perNames[RandomInt(0, perNames.Length)]}";
            }
            while (tempUsedNames.IndexOf(name) != -1 || usedNames.IndexOf(name) != -1);

            tempUsedNames.Add(name);

            __result = name;

            return false;
        }

        /// <summary>
        /// 生成移民列表
        /// </summary>
        public static void CitizenCaveUI_MakeCitizenList_CustomName()
        {
            if (!ActiveCustomNames)
                return;

            tempUsedNames.Clear();
        }

        /// <summary>
        /// 生成市民
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_info"></param>
        public static void T_Citizen_MakeCtizen_ByCC_CustomName(T_Citizen __instance, CCMake_Info _info)
        {
            if (!ActiveCustomNames || TryGetSpecialUnit(__instance, out _))
                return;

            usedNames.Add(_info.Name);

            Debug.Log($"添加了自定义市民 {_info.Name}");
        }
        #endregion

        #region AI相关

        /// <summary>
        /// 默认的需求阈值
        /// </summary>
        public static Dictionary<string, float> DefaultDesireDatas = new Dictionary<string, float>
        {
            { "FatigueCut", 0f},
            { "HungerCut", 0f},
            { "FunCut", 0f},
            { "CleanCut", 0f},
        };

        /// <summary>
        /// 恢复阈值
        /// </summary>
        public static float[] RestoreThresholdValues = new float[] { 50, 60, 70 };
        /// <summary>
        /// 恢复目标值
        /// </summary>
        public static float[] RestoreTargetValues = new float[] { 80, 100, 120 };
        /// <summary>
        /// 最大需求值
        /// </summary>
        public static float[] MaxDesireValues = new float[] { 100, 125, 150 };

        /// <summary>
        /// 市民需求阈值
        /// </summary>
        static Dictionary<T_Citizen, CitizenDesireThreshold> CitizenDesires = new Dictionary<T_Citizen, CitizenDesireThreshold>();

        /// <summary>
        /// 设置需求原阈值
        /// </summary>
        /// <param name="set"></param>
        static void SetDesireCut(bool set = false)
        {
            for (int i = 0; i < DefaultDesireDatas.Count; i++)
            {
                KeyValuePair<string, float> keyValue = DefaultDesireDatas.ElementAt(i);

                if (set)
                    DefinesSetValue(keyValue.Key, keyValue.Value);
                else
                    DefaultDesireDatas[keyValue.Key] = DefinesGetValue<float>(keyValue.Key);
            }
        }

        /// <summary>
        /// 更新需求原阈值
        /// </summary>
        static void UpdateDesireCut(float? value = null)
        {
            DefinesSetValue("FatigueCut", 20f);
            DefinesSetValue("HungerCut", value ?? 999f);
            DefinesSetValue("FunCut", value ?? 999f);
            DefinesSetValue("CleanCut", value ?? 999f);
        }

        /// <summary>
        /// 检查所有需求
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        public static bool T_Citizen_Check_AllDesire(T_Citizen __instance)
        {
            if (!ActiveOptimizeAI)
                return true;

            if (__instance.m_Buff.IsExist(C_Buff.CombatMode))
                return false;

            CitizenDesireThreshold desire = GetCitizenDesire(__instance);

            //float LifeValue = __instance.List_LifeDesire.Sum(t => t.Value);

            __instance.Do_Sleep();

            if (__instance.result_num == -1)
            {
                //Debug.LogWarning($"{__instance.m_UnitName} 需要吃饭 {desire.NeedRestoreHunger}，当前饥饿 {__instance.m_Hunger}， {desire.restoringHunger}/{desire.restoringFun}/{desire.restoringClean}");

                if (desire.NeedRestoreHunger)
                {
                    __instance.Do_Eating();

                    if (__instance.result_num != -1)
                        desire.restoringHunger = true;
                }
                else
                    desire.restoringHunger = false;
            }
            if (__instance.result_num == -1)
            {
                if (desire.NeedRestoreFun)
                {
                    __instance.Do_Fun();

                    if (__instance.result_num != -1)
                        desire.restoringFun = true;

                    //Debug.Log($"{__instance.m_UnitName} 正在娱乐({__instance.List_State.Count}) {__instance.m_Fun} / {desire.restoringFun}");
                }
                else
                    desire.restoringFun = false;
            }
            if (__instance.result_num == -1)
            {
                if (desire.NeedRestoreClean)
                {
                    __instance.Do_Clean();

                    if (__instance.result_num != -1)
                        desire.restoringClean = true;
                }
                else
                    desire.restoringClean = false;
            }
            if (__instance.result_num == -1)
            {
                __instance.Do_Life();
            }

            return false;
        }

        /// <summary>
        /// 解除睡觉
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_natural"></param>
        /// <returns></returns>
        public static bool T_Citizen_SleepRelease(T_Citizen __instance, bool _natural)
        {
            if (!ActiveOptimizeAI || !_natural)
                return true;

            return __instance.m_Fatigue >= 100f;
        }

        /// <summary>
        /// 更新食物值
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        public static bool T_Citizen_HungerUpdate_AI(T_Citizen __instance, float value)
        {
            if (!ActiveOptimizeAI)
                return true;

            UpdateDesireValue(__instance, __instance.m_Buff.GetBuffValue(C_Buff.HUG_Up) + __instance.m_Buff.GetBuffValue(C_Buff.HUG_Down), ref value);

            CitizenDesireThreshold desire = SetDesireValue(__instance, ref __instance.m_Hunger, value);

            //Debug.Log($"{__instance.m_UnitName} 更新食物值为 {__instance.m_Hunger} / {desire.MaxDesireValue}");

            if (!__instance.m_Buff.IsExist(C_Buff.DieHunger))
            {
                if (__instance.m_Hunger == 0f)
                    __instance.m_Buff.BuffRefSet(C_Buff.DieHunger, "", C_Buff_Category.None, 1f, 24, true, true);
            }
            else if (__instance.m_Hunger > 0f)
                __instance.m_Buff.BuffKill(C_Buff.DieHunger);

            return false;
        }

        /// <summary>
        /// 更新娱乐值
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        public static bool T_Citizen_FunUpdate_AI(T_Citizen __instance, float value)
        {
            if (!ActiveOptimizeAI)
                return true;

            UpdateDesireValue(__instance, __instance.m_Buff.GetBuffValue(C_Buff.FUN_Up) + __instance.m_Buff.GetBuffValue(C_Buff.FUN_Down), ref value);

            CitizenDesireThreshold desire = SetDesireValue(__instance, ref __instance.m_Fun, value);

            //if (value > 0)
            //    Debug.Log($"{__instance.m_UnitName}({desire.Grade}) 更新娱乐值为 {__instance.m_Fun}({desire.NeedRestoreFun}) / {desire.RestoreTargetValue} / {desire.MaxDesireValue}");

            return false;
        }

        /// <summary>
        /// 更新卫生值
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        public static bool T_Citizen_CleanUpdate_AI(T_Citizen __instance, float value)
        {
            if (!ActiveOptimizeAI)
                return true;

            UpdateDesireValue(__instance, __instance.m_Buff.GetBuffValue(C_Buff.CLEAN_Up) + __instance.m_Buff.GetBuffValue(C_Buff.CLEAN_Down), ref value);

            CitizenDesireThreshold desire = SetDesireValue(__instance, ref __instance.m_Cleanliness, value);

            //if (value > 0)
            //    Debug.Log($"{__instance.m_UnitName}({desire.Grade}) 更新卫生值为 {__instance.m_Cleanliness}({desire.NeedRestoreClean}) / {desire.RestoreTargetValue} / {desire.MaxDesireValue}");

            return false;
        }

        /// <summary>
        /// 更新需求值
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="updateValue"></param>
        /// <param name="value"></param>
        static void UpdateDesireValue(T_Citizen citizen, float updateValue, ref float value)
        {
            if (value < 0f)
            {
                float num = 1f + updateValue;

                num = num == 0 ? 1f : num < 0f ? 0.01f : num;

                value *= num;
            }
            else
                citizen.PDI_Calculate();
        }

        /// <summary>
        /// 设置需求值
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="desireValue"></param>
        /// <param name="value"></param>
        static CitizenDesireThreshold SetDesireValue(T_Citizen citizen, ref float desireValue, float value)
        {
            CitizenDesireThreshold desire = GetCitizenDesire(citizen);

            desireValue += value;

            if (desireValue < 0f)
                desireValue = 0f;
            else if (desireValue > desire.MaxDesireValue)
                desireValue = desire.MaxDesireValue;

            return desire;
        }

        /// <summary>
        /// 检查气泡状态
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        public static void SpeechBubble_Check_Desire(SpeechBubble __instance)
        {
            if (!ActiveOptimizeAI)
                return;

            CitizenDesireThreshold desire = GetCitizenDesire(__instance.m_Master);

            UpdateDesireCut(desire.RestoreThresholdValue);
        }

        /// <summary>
        /// 检查气泡状态
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        public static void SpeechBubble_Check_Desire_Post(SpeechBubble __instance)
        {
            if (!ActiveOptimizeAI)
                return;

            UpdateDesireCut();
        }

        ///// <summary>
        ///// 饥饿界面设置值
        ///// </summary>
        ///// <param name="__instance"></param>
        ///// <returns></returns>
        //[HarmonyPostfix, HarmonyPatch(typeof(StatisticsSlotModuleFoodUI), "SetData")]
        //public static void StatisticsSlotModuleFoodUI_SetData(T_Citizen citizen, GaugeSlotUI ____gaugeSlotUI)
        //{
        //    if (!ActiveOptimizeAI || citizen == null)
        //        return;

        //    ____gaugeSlotUI.SetText(citizen.m_Hunger.ToString("F0"));
        //}

        ///// <summary>
        ///// 娱乐界面设置值
        ///// </summary>
        ///// <param name="__instance"></param>
        ///// <returns></returns>
        //[HarmonyPostfix, HarmonyPatch(typeof(StatisticsSlotModuleFunUI), "SetData")]
        //public static void StatisticsSlotModuleFunUI_SetData(T_Citizen citizen, GaugeSlotUI ____gaugeSlotUI)
        //{
        //    if (!ActiveOptimizeAI || citizen == null)
        //        return;

        //    ____gaugeSlotUI.SetText(citizen.m_Fun.ToString("F0"));
        //}

        ///// <summary>
        ///// 卫生界面设置值
        ///// </summary>
        ///// <param name="__instance"></param>
        ///// <returns></returns>
        //[HarmonyPostfix, HarmonyPatch(typeof(StatisticsSlotModuleCleanUI), "SetData")]
        //public static void StatisticsSlotModuleCleanUI_SetData(T_Citizen citizen, GaugeSlotUI ____gaugeSlotUI)
        //{
        //    if (!ActiveOptimizeAI || citizen == null)
        //        return;

        //    ____gaugeSlotUI.SetText(citizen.m_Cleanliness.ToString("F0"));
        //}

        /// <summary>
        /// 需求信息栏
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        public static void SummaryDesireSlotUI_SetDataInternal(SummaryDesireSlotUI __instance, T_Citizen citizen, DesireInfoType ____typeDesireInfo, TextMeshProUGUI ____desireValue)
        {
            if (!ActiveOptimizeAI || citizen == null)
                return;

            float value;

            if (____typeDesireInfo == DesireInfoType.Hunger)
                value = citizen.m_Hunger;
            else if (____typeDesireInfo == DesireInfoType.Fun)
                value = citizen.m_Fun;
            else if (____typeDesireInfo == DesireInfoType.Cleanness)
                value = citizen.m_Cleanliness;
            else
                return;

            ____desireValue.text = $" {value.ToString("F0")}/{GetCitizenDesire(citizen).MaxDesireValue}";

            //Debug.Log($"{citizen.m_UnitName} {____typeDesireInfo} 值为 {value}");
        }

        /// <summary>
        /// 获得市民需求
        /// </summary>
        /// <param name="citizen"></param>
        /// <returns></returns>
        static CitizenDesireThreshold GetCitizenDesire(T_Citizen citizen)
        {
            if (!CitizenDesires.TryGetValue(citizen, out CitizenDesireThreshold desire))
            {
                desire = new CitizenDesireThreshold(citizen);

                CitizenDesires.Add(citizen, desire);
            }

            return desire;
        }

        #endregion

        #region 饭桌优化

        /// <summary>
        /// 执行吃饭
        /// </summary>
        /// <param name="__instance"></param>
        //[HarmonyPrefix, HarmonyPatch(typeof(T_Citizen), "Do_Eating")]
        //public static bool T_Citizen_Do_Eating(T_Citizen __instance)
        //{
        //    if (!ActiveOptimizeFoodTable)
        //        return true;

        //    __instance.result_num = -1;
        //    __instance.m_SpeechBubble.SelfCheck();
        //    if (__instance.m_ImFatigue > 0)
        //    {
        //        return false;
        //    }

        //    //达到饥饿阈值，且存在可以购买食物的建筑（重置食物的占用状态）
        //    if (__instance.m_Hunger < DefinesGetValue<float>("HungerCut") && BuildingMgr.IsExistBuyFoodSpot(__instance))
        //    {
        //        bool isWorkingOrMoving = Timing.IsRunning(__instance._corWorking) || __instance.IsMoveState();

        //        if (GetPrivateValue(__instance).Method("Desire_GatheringListCheck").GetValue<bool>())
        //        {
        //            return false;
        //        }
        //        if (__instance.List_State.Count == 0 && !isWorkingOrMoving)
        //        {
        //            //string name = __instance.m_TargetBuilding != null ? __instance.m_TargetBuilding.m_CustomName : "无";

        //            //Debug.Log($"{__instance.m_UnitName} 正寻找食物建筑目标 {name}");

        //            if (__instance.m_TargetBuilding == null)
        //            {
        //                BuildingMgr.FindFoodBuilding(__instance);
        //                return false;
        //            }
        //        }
        //        else if (__instance.List_State.Count != 0)
        //        {
        //            CitizenState state = __instance.List_State.Count > 0 ? __instance.List_State[0] : CitizenState.None;

        //            //string name = __instance.m_TargetBuilding != null ? __instance.m_TargetBuilding.m_CustomName : "无";

        //            //Debug.Log($"{__instance.m_UnitName} 当前状态 {state} 目标 {name}");

        //            //使用饭桌中：到达饭桌
        //            if (state == CitizenState.UsingTable && !isWorkingOrMoving)
        //            {
        //                //Debug.Log($"{__instance.m_UnitName} 正使用饭桌 {name}");

        //                //++ 购买食物
        //                if (!BuyFoodFromTable(__instance))
        //                {
        //                    __instance.BehaviorStop(true);
        //                    __instance.m_TargetBuilding = null;
        //                    __instance.result_num = -1;
        //                    return false;
        //                }
        //                //建筑状态检查，进行服务（建筑交互）
        //                if (Helpers.BuildSafeCheck(ref __instance.m_TargetBuilding))
        //                {
        //                    //Debug.LogWarning($"{__instance.m_UnitName} 进入饭桌 {__instance.m_TargetBuilding.m_CustomName}");

        //                    __instance.m_TargetBuilding.m_BuildInfoUI.SellService(__instance, false);
        //                    return false;
        //                }
        //                //吃完饭了，重置
        //                if (__instance.m_Food == TileType.None || __instance.m_TargetBuilding == null)
        //                {
        //                    __instance.BehaviorStop(true);
        //                    __instance.result_num = -1;
        //                    Debug.LogWarning($"{__instance.m_UnitName} 结束吃饭？");
        //                    return false;
        //                }
        //                __instance.BehaviorStop(true);
        //                return false;
        //            }
        //            //吃饭中：到达食物目标建筑（仓库）后
        //            else if (state == CitizenState.Eating && !isWorkingOrMoving)
        //            {
        //                return true;

        //                //IEnumerator<float> coroutine = GetPrivateValue(__instance).Method("Do_EatingC").GetValue<IEnumerator<float>>();

        //                //name = __instance.m_TargetBuilding != null ? __instance.m_TargetBuilding.m_CustomName : "无";

        //                //Debug.LogWarning($"{__instance.m_UnitName} 到达了 {name}");

        //                ////3. 已购买食物时，执行吃饭（购买了食物后，进入饭桌动画交互后？）
        //                //if (__instance.m_Food != TileType.None)
        //                //{
        //                //    __instance._corUseBuilding = Timing.RunCoroutine(coroutine);
        //                //    __instance.result_num = 0;

        //                //    Debug.LogWarning($"{__instance.m_UnitName} 正在吃 {__instance.m_Food}");
        //                //    return false;
        //                //}
        //                ////0. 目标建筑无法使用时，停止当前行为
        //                //if (!Helpers.BuildSafeCheck(ref __instance.m_TargetBuilding) || !__instance.m_TargetBuilding.IsInArea(1, __instance.m_CurNode.GetIntPos()))
        //                //{
        //                //    __instance.BehaviorStop(true);
        //                //    return false;
        //                //}
        //                ////1. 购买食物，若失败则重置目标
        //                //if (!__instance.m_TargetBuilding.BuyFood(__instance))
        //                //{
        //                //    __instance.m_TargetBuilding = null;
        //                //    __instance.result_num = -1;
        //                //    return false;
        //                //}
        //                ////2. 查找饭桌，若失败则直接吃饭
        //                //if (GameMgr.Instance._BuildingMgr.FindTableBuilding(__instance) == -1)
        //                //{
        //                //    __instance._corUseBuilding = Timing.RunCoroutine(coroutine);
        //                //    __instance.result_num = 0;
        //                //    Debug.LogWarning($"{__instance.m_UnitName} 没有找到饭桌，正在吃 {__instance.m_Food}");
        //                //    return false;
        //                //}
        //                //__instance.result_num = 0;
        //                //return false;
        //            }
        //            //工作中
        //            else if (state == CitizenState.Working)
        //            {
        //                BuildingMgr.FindFoodBuilding(__instance);
        //            }
        //        }
        //    }

        //    return false;
        //}

        ///// <summary>
        ///// 从饭桌购买食物
        ///// </summary>
        ///// <param name="_unit"></param>
        ///// <returns></returns>
        //static bool BuyFoodFromTable(T_Citizen _unit, Building table = null)
        //{
        //    if (table == null)
        //        table = _unit.m_TargetBuilding;

        //    if (table == null || table.m_Info.Name != BuildingName.Table)
        //    {
        //        string name = table != null ? table.m_CustomName : "无";

        //        Debug.LogWarning($"{_unit.m_UnitName} 获取餐桌 {name} 失败");

        //        return false;
        //    }

        //    if (_unit.m_TargetFood == TileType.None && !GetTargetFood(_unit))
        //    {
        //        Debug.LogWarning($"{_unit.m_UnitName} 获取食物失败");

        //        return false;
        //    }

        //    //从绑定的目标仓库进行购买，失败时，重新检索所有仓库和需求
        //    if (TableUsedStorages.TryGetValue(_unit.m_ID, out Building tableStorage) && BuyFoodFromStorage(_unit, tableStorage, _unit.m_TargetFood))
        //    {
        //        //Debug.Log($"{_unit.m_UnitName} 从 {table.m_CustomName}({tableStorage.m_CustomName}) 购买 {_unit.m_TargetFood} 成功");

        //        return true;
        //    }

        //    CitizenDesireThreshold desireThreshold = GetCitizenDesire(_unit);

        //    List<DesireInfo> desires = _unit.List_HungerDesire.OrderBy(t => Mathf.Abs(desireThreshold.NeedRestoreHungerValue - (DBMgr.Dic_TileDB.TryGetValue(t.NeedType, out TileInfo tileInfo) ? tileInfo.Value : 0))).ToList();

        //    List<Building_Storage> storages = ActiveShareStorage ? new List<Building_Storage> { BuildingMgr.List_Storage[0] } : GetStoragesByDesires(desires, (int)_unit.m_Grade);

        //    //根据直线距离排序最近的仓库
        //    storages.Select(t => t as Building).ToList().Sort((Building x1, Building x2) => Helpers.Distance(_unit.Tf.position, x1.Tf.position).CompareTo(Helpers.Distance(_unit.Tf.position, x2.Tf.position)));

        //    TileSt_Info info;

        //    //Debug.LogWarning($"{_unit.m_UnitName} 共 {desires.Count} 可选食物，{storages.Count} 可选仓库");

        //    //迭代所有符合需求的仓库
        //    foreach (Building_Storage storage in storages)
        //    {
        //        //迭代需求
        //        foreach (DesireInfo desire in desires)
        //        {
        //            info = storage.List_TileObj.Find(t => t.m_Type == desire.NeedType);

        //            if (info != null && BuyFoodFromStorage(_unit, storage, info.m_Type))
        //            {
        //                //Debug.LogWarning($"{_unit.m_UnitName} 从 {table.m_CustomName}({storage.m_CustomName}) 购买 {info.m_Type}/{_unit.m_TargetFood} 成功");

        //                return true;
        //            }
        //        }
        //    }

        //    Debug.LogWarning($"{_unit.m_UnitName} 从 {table.m_CustomName} 购买 {_unit.m_TargetFood} 失败");

        //    return false;
        //}

        ///// <summary>
        ///// 从仓库购买食物
        ///// </summary>
        ///// <param name="_unit"></param>
        ///// <param name="storage"></param>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //static bool BuyFoodFromStorage(T_Citizen _unit, Building storage, TileType type)
        //{
        //    float price = EcoMgr.GetPrice(type);

        //    //可以购买时
        //    if (_unit.CanI_Buy(price))
        //    {
        //        //移除该物品
        //        if (storage.RemoveValue(type, _unit.m_ID) || storage.RemoveValue(type, 0))
        //        {
        //            _unit.m_Food = type;

        //            EcoMgr.BuyForRat_ForC_Resource(_unit, price, type);

        //            return true;
        //        }
        //    }

        //    return false;
        //}

        ///// <summary>
        ///// 出售服务
        ///// </summary>
        ///// <param name="_unit"></param>
        ///// <param name="_value"></param>
        ///// <returns></returns>
        //[HarmonyPrefix, HarmonyPatch(typeof(ToiletInfo), "SellService", new Type[] { typeof(T_Citizen), typeof(bool) })]
        //public static bool ToiletInfo_SellService(ToiletInfo __instance, T_Citizen _unit, bool _value, ref bool __result)
        //{
        //    if (!ActiveOptimizeFoodTable || __instance.m_Building.m_Info.Name != BuildingName.Table)
        //        return true;

        //    __result = true;

        //    _unit.result_num = 0;

        //    if (__instance.m_Building.IsInArea(1, _unit.m_CurNode.GetIntPos()))
        //        _unit._corUseBuilding = Timing.RunCoroutine(SellTableC(__instance, _unit));

        //    return false;
        //}

        //public static IEnumerator<float> SellTableC(ToiletInfo __instance, T_Citizen _unit)
        //{
        //    _unit.m_UseBuildingStep = UseBuildingStep.UsingBuilding;
        //    _unit.RemoveWeapon();
        //    _unit.m_WorkState = 1;
        //    //TileType _effect_type = _unit.m_Food;
        //    //if (DBMgr.Dic_TileDB.ContainsKey(_unit.m_Food) && !PlayDataMgr.Instance.IsLoadGame)
        //    //{
        //    //    TileInfo tileInfo = DBMgr.Dic_TileDB[_unit.m_Food];
        //    //    _unit.HungerUpdate(tileInfo.Value);
        //    //    if (_unit.m_Buff.IsExistRef(__instance.m_Building.m_Info.EffectValue3))
        //    //        _unit.m_Buff.RefKill(__instance.m_Building.m_Info.EffectValue3);
        //    //    for (int i = 0; i < __instance.m_Building.m_Info.List_EffectAbility.Count; i++)
        //    //    {
        //    //        C_Buff buff = CitizenBuff.ResToBuff(__instance.m_Building.m_Info.List_EffectAbility[i], __instance.m_Building.m_Info.List_EffectAbilityValue[i]);
        //    //        _unit.m_Buff.BuffRefSet(buff, __instance.m_Building.m_Info.Name.ToString(), C_Buff_Category.None, __instance.m_Building.m_Info.List_EffectAbilityValue[i], __instance.m_Building.m_Info.EffectValue2_Num, false, true);
        //    //    }
        //    //    _unit.ApplyFoodOrLife_ResAbility(tileInfo);
        //    //    _unit.ApplyFunOrClean_ResAbility(__instance.m_Building.m_Info, -999, true);
        //    //}
        //    int _index = __instance.m_Building.List_Guest.FindIndex((T_Citizen x) => x.m_UnitName == _unit.m_UnitName);
        //    if (__instance.m_Building.List_Guest.Count == 1)
        //        _index = 0;
        //    else if (__instance.m_Building.List_Guest.Count >= 2)
        //    {
        //        T_Citizen t_Citizen = __instance.m_Building.List_Guest.Find((T_Citizen x) => x != _unit);
        //        if (t_Citizen != null && t_Citizen._corUseBuilding.IsRunning)
        //            _index = t_Citizen.Tf.position.x < __instance.m_Building.Tf.position.x ? 1 : 0;
        //    }
        //    if (_index == 0)
        //    {
        //        _unit.SetZ_Order(true);
        //        _unit.FlipX(_unit.Tf.position.x < __instance.m_Building.Tf.position.x - 0.7f);
        //        _unit._cormoveInfo = Func.Instance.cocos2d.T_MoveBy(new Vector2(__instance.m_Building.Tf.position.x - 0.7f, _unit.Tf.position.y), 0.3f, ref _unit.Obj);
        //    }
        //    else
        //    {
        //        _unit.SetZ_Order(true);
        //        _unit.FlipX(_unit.Tf.position.x < __instance.m_Building.Tf.position.x + 0.7f);
        //        _unit._cormoveInfo = Func.Instance.cocos2d.T_MoveBy(new Vector2(__instance.m_Building.Tf.position.x + 0.7f, _unit.Tf.position.y), 0.3f, ref _unit.Obj);
        //    }
        //    _unit.SetAniState(AniState.Walking, true, false);
        //    while (!_unit._cormoveInfo.end && _unit.m_SkipIndex == 0)
        //    {
        //        yield return 0f;
        //    }
        //    _unit.FlipX(_index == 0);

        //    float t_time = 0f;

        //    do
        //    {
        //        TileType _effect_type = _unit.m_Food;
        //        if (DBMgr.Dic_TileDB.TryGetValue(_unit.m_Food, out TileInfo tileInfo) && !PlayDataMgr.Instance.IsLoadGame)
        //        {
        //            _unit.HungerUpdate(tileInfo.Value);
        //            if (_unit.m_Buff.IsExistRef(__instance.m_Building.m_Info.EffectValue3))
        //                _unit.m_Buff.RefKill(__instance.m_Building.m_Info.EffectValue3);
        //            for (int i = 0; i < __instance.m_Building.m_Info.List_EffectAbility.Count; i++)
        //            {
        //                C_Buff buff = CitizenBuff.ResToBuff(__instance.m_Building.m_Info.List_EffectAbility[i], __instance.m_Building.m_Info.List_EffectAbilityValue[i]);
        //                _unit.m_Buff.BuffRefSet(buff, __instance.m_Building.m_Info.Name.ToString(), C_Buff_Category.None, __instance.m_Building.m_Info.List_EffectAbilityValue[i], __instance.m_Building.m_Info.EffectValue2_Num, false, true);
        //            }
        //            _unit.ApplyFoodOrLife_ResAbility(tileInfo);
        //            _unit.ApplyFunOrClean_ResAbility(__instance.m_Building, -999, true);
        //        }

        //        int index = _index == 0 ? 2 : 3;

        //        __instance.m_Building.m_Body.m_BuildParts[index].gameObject.SetActive(true);
        //        __instance.m_Building.m_Body.m_BuildParts[index].SetSkin(Func.Instance.LoadSprite(string.Format("{0}Object", _effect_type)));

        //        _unit.SetAniState(AniState.Eating, "Eating_Table", false, false);

        //        if (DBMgr.Dic_TileDB.ContainsKey(_effect_type))
        //        {
        //            PoolMgr.Pool_GetEffect.GetNextObj().GetComponent<GetEffect>().GetKindEffect(GetKind.Hunger, _unit, GameMgr.Instance._DB_Mgr.Dic_TileDB[_effect_type].Value, new Vector3(0f, 0.5f, 0f));
        //        }

        //        t_time = 0f;

        //        while (t_time < 6.4f && _unit.m_SkipIndex == 0)
        //        {
        //            t_time += Timing.DeltaTime;

        //            yield return 0f;
        //        }
        //    }
        //    while (ContinueEat(_unit, __instance.m_Building));

        //    __instance.m_Building.m_Body.m_BuildParts[2].gameObject.SetActive(false);
        //    __instance.m_Building.m_Body.m_BuildParts[3].gameObject.SetActive(false);
        //    t_time = 0f;
        //    while (t_time < 1f && _unit.m_SkipIndex == 0)
        //    {
        //        t_time += Timing.DeltaTime;
        //        yield return 0f;
        //    }
        //    _unit.m_WorkState = 0;
        //    _unit.m_Food = TileType.None;
        //    _unit.m_TargetFood = TileType.None;
        //    _unit.SetZ_Order(false);
        //    __instance.m_Building.List_Guest.Remove(_unit);
        //    _unit.BehaviorStop(true);
        //    __instance.m_Building.m_DailyCustomer++;
        //    yield break;
        //}

        ///// <summary>
        ///// 继续吃
        ///// </summary>
        ///// <param name="unit"></param>
        ///// <param name="table"></param>
        ///// <returns></returns>
        //static bool ContinueEat(T_Citizen unit, Building table)
        //{
        //    if (GetTargetFood(unit) && BuyFoodFromTable(unit, table))
        //    {
        //        Debug.LogWarning($"{unit.m_UnitName} 又吃了一个 {TileName(unit.m_Food, true)}/{TileName(unit.m_TargetFood, true)}");

        //        return true;
        //    }

        //    return false;
        //}

        ///// <summary>
        ///// 获得目标食物
        ///// </summary>
        ///// <param name="unit"></param>
        ///// <returns></returns>
        //static bool GetTargetFood(T_Citizen unit)
        //{
        //    CitizenDesireThreshold desire = GetCitizenDesire(unit);

        //    if (!desire.NeedRestoreHunger)
        //        return false;

        //    unit.List_HungerDesire = unit.List_HungerDesire.OrderBy(t => Mathf.Abs(desire.NeedRestoreHungerValue - (DBMgr.Dic_TileDB.TryGetValue(t.NeedType, out TileInfo info) ? info.Value : 0))).ToList();

        //    unit.m_TargetFood = unit.List_HungerDesire[0].NeedType;

        //    return true;
        //}

        #endregion

        #region 寻路相关、饭桌优化

        /// <summary>
        /// 寻找最短访客建筑
        /// </summary>
        /// <param name="_unit"></param>
        /// <param name="_name"></param>
        /// <param name="__result"></param>
        public static bool BuildingMgr_FindShortestGuestBuilding(GameUnit _unit, BuildingName _name, ref Building __result)
        {
            if (!ActiveOptimizeGuestPathFind)
                return true;

            int grade = (int)(_unit as T_Citizen).m_Grade;

            BuildingMgr buildingMgr = BuildingMgr;

            List<Building> buildings = buildingMgr.List_Building;

            Building building;

            List<Building> list = new List<Building>();

            for (int i = 0; i < buildings.Count; i++)
            {
                building = buildings[i];

                //目标建筑、店主在、有位置、阶级可用、在范围内、钱够、非工作地、正常状态、未屏蔽
                if (building.m_Info.Name == _name && building.m_BuildInfoUI.IsMasterReady() && building.m_BuildInfoUI.IsGuestReady() && Helpers.GradeEnableCheck(DBMgr.Dic_BuildDB[building.m_Info.Name].Level, grade) && building.m_BuildInfoUI.IsInRange(_unit.Tf.position) && building.m_Info.EffectValue1_Num <= _unit.m_Gold && building != _unit.m_Job && building.m_BuildState == BuildState.Basic && !_unit.List_E_ClosedMark.Contains(building.m_ID))
                {
                    list.Add(building);
                }
                else if (!building.m_BuildInfoUI.IsGuestReady() && building.m_Info.Desire_Name != DesireName.None)
                {
                    if (building.m_Info.Desire_Name == DesireName.Clean)
                    {
                        for (int j = 0; j < building.List_Guest.Count; j++)
                        {
                            if (!building.List_Guest[j].List_State.Contains(CitizenState.Using_Clean))
                            {
                                building.List_Guest.RemoveAt(j);
                            }
                        }
                    }
                    else if (building.m_Info.Desire_Name == DesireName.Fun)
                    {
                        for (int k = 0; k < building.List_Guest.Count; k++)
                        {
                            if (!building.List_Guest[k].List_State.Contains(CitizenState.Using_Fun))
                            {
                                building.List_Guest.RemoveAt(k--);
                            }
                        }
                    }
                    else if (building.m_Info.Desire_Name == DesireName.Anything || building.m_Info.Desire_Name == DesireName.Happy)
                    {
                        for (int l = 0; l < building.List_Guest.Count; l++)
                        {
                            if (building.m_Info.Name == BuildingName.Hospital && !building.List_Guest[l].List_State.Contains(CitizenState.Hospital_Use))
                            {
                                building.List_Guest.RemoveAt(l--);
                            }
                        }
                    }
                }
            }

            TryFindShortestBuilding(_unit, _unit.m_CurNode.GetPos(), list, out __result);

            return false;
        }

        /// <summary>
        /// 寻找日用品建筑
        /// </summary>
        /// <param name="_unit"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool BuildingMgr_FindLifeBuilding(T_Citizen _unit, ref int __result)
        {
            if (!ActiveOptimizeFoodAndLifePathFind)
                return true;

            FindStorageBuilding(_unit, _unit.List_LifeDesire, ref __result, ref _unit.m_TargetLife, false);

            return false;
        }

        /// <summary>
        /// 寻找食物建筑
        /// </summary>
        /// <param name="_unit"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool BuildingMgr_FindFoodBuilding(T_Citizen _unit, ref int __result)
        {
            if (!ActiveOptimizeFoodAndLifePathFind)
                return true;

            CitizenDesireThreshold desire = GetCitizenDesire(_unit);

            _unit.List_HungerDesire = _unit.List_HungerDesire.OrderBy(t => Mathf.Abs(desire.NeedRestoreHungerValue - (DBMgr.Dic_TileDB.TryGetValue(t.NeedType, out TileInfo info) ? info.Value : 0))).ToList();

            List<string> list = _unit.List_HungerDesire.Select(t => $"{TileName(t.NeedType, true)}").ToList();

            FindStorageBuilding(_unit, _unit.List_HungerDesire, ref __result, ref _unit.m_TargetFood, true, false);

            //string name = _unit.m_TargetBuilding != null ? _unit.m_TargetBuilding.m_CustomName : "无";

            //Debug.LogWarning($"{_unit.m_UnitName} 前往 {name} 恢复饥饿值 {desire.NeedRestoreHungerValue}，目标食物 {TileName(_unit.m_TargetFood)}/{string.Join(",", list)}");

            return false;
        }

        /// <summary>
        /// 寻找仓库建筑
        /// </summary>
        /// <param name="_unit"></param>
        /// <param name="desires"></param>
        /// <param name="__result"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        static bool FindStorageBuilding(T_Citizen _unit, List<DesireInfo> desires, ref int __result, ref TileType type, bool isFood, bool isFindTable = false)
        {
            int grade = (int)_unit.m_Grade;

            __result = -1;

            type = TileType.None;

            List<Building_Storage> storages = GetStoragesByDesires(desires, grade);

            if (storages.Count == 0)
                return false;

            //获得寻路的目标建筑列表
            List<Building> list = new List<Building>();

            Building target = null;

            if (isFindTable)
            {
                list = GetServiceBuildings(_unit, BuildingName.Table, CitizenState.UsingTable);

                if (!TryFindShortestBuilding(_unit, _unit.m_CurNode.GetPos(), list, out target))
                    list.Clear();
            }

            //未找到桌子或寻找仓库
            if (list.Count == 0 || !isFindTable)
            {
                list = storages.Select(t => t as Building).ToList();

                isFindTable = false;
            }

            //Debug.LogWarning($"{_unit.m_UnitName} 购买 {_mat}，共 {list.Count} 可选目标仓库");

            if (target == null && !TryFindShortestBuilding(_unit, _unit.m_CurNode.GetPos(), list, out target))
            {
                //Debug.LogWarning($"{_unit.m_UnitName} 寻找最近目标失败，共 {list.Count} 可选目标");

                return false;
            }

            //寻找桌子时，绑定直线距离最近的仓库的食物
            if (isFindTable)
            {
                //根据与桌子直线距离排序最近的仓库
                storages.Select(t => t as Building).ToList().Sort((Building x1, Building x2) => Helpers.Distance(target.Tf.position, x1.Tf.position).CompareTo(Helpers.Distance(target.Tf.position, x2.Tf.position)));

                foreach (Building_Storage storage in storages)
                {
                    //绑定目标占用
                    if (MoveToTargetAndBindReservation(_unit, desires, target, ref __result, ref type, isFood, storage))
                    {
                        string buildName = _unit.m_TargetBuilding != null ? _unit.m_TargetBuilding.m_CustomName : "无";

                        //Debug.Log($"{_unit.m_UnitName} 目标桌子 {target.m_CustomName}({storage.m_CustomName}) 目标食物 {TileName(type)}，当前移动目标 {buildName}");

                        return true;
                    }
                }

                Debug.LogWarning($"{_unit.m_UnitName} 寻找桌子吃饭失败");
            }
            else
                return MoveToTargetAndBindReservation(_unit, desires, target, ref __result, ref type, isFood);

            __result = -1;

            return false;
        }

        /// <summary>
        /// 获得符合需求的仓库
        /// </summary>
        /// <param name="desires"></param>
        /// <param name="grade"></param>
        /// <returns></returns>
        static List<Building_Storage> GetStoragesByDesires(List<DesireInfo> desires, int grade)
        {
            return ActiveShareStorage ? BuildingMgr.List_Storage : BuildingMgr.List_Storage.Where(t => t.List_TileObj.Find(x => desires.Find(h => h.NeedType == x.m_Type && Helpers.GradeEnableCheck(DBMgr.Dic_TileDB[x.m_Type].Level, grade)) != null) != null).ToList();
        }

        /// <summary>
        /// 获得服务建筑
        /// </summary>
        /// <param name="buildName"></param>
        /// <param name="state"></param>
        /// <param name="grade"></param>
        /// <returns></returns>
        static List<Building> GetServiceBuildings(T_Citizen unit, BuildingName buildName, CitizenState state)
        {
            List<Building> list = new List<Building>();

            BuildInfo info;

            BuildInfoUI infoUI;

            foreach (Building building in BuildingMgr.List_Building)
            {
                info = building.m_Info;

                if (info.Name != buildName)
                    continue;

                if (!Helpers.GradeEnableCheck(info.Level, (int)unit.m_Grade) || !Helpers.IsDistanceSmaller(unit.Tf.position, building.Tf.position, building.m_Range))
                    continue;

                infoUI = building.m_BuildInfoUI;

                if (!infoUI.IsGuestReady())
                {
                    T_Citizen guest;

                    for (int i = 0; i < building.List_Guest.Count; i++)
                    {
                        guest = building.List_Guest[i];

                        if (guest.m_CharState != CharState.None || !guest.Obj.activeSelf || !guest.List_State.Contains(state))
                        {
                            if (state == CitizenState.UsingTable && guest.m_TargetFood != TileType.None)
                                continue;

                            building.List_Guest.RemoveAt(i--);

                            Debug.Log($"{building.m_CustomName} 移除了访客 {guest.m_UnitName} 当前共有 {building.List_Guest.Count} 访客");
                        }
                    }

                    continue;
                }

                Debug.Log($"{unit.m_UnitName} 目标桌子 {building.m_CustomName} 共有 {building.List_Guest.Count} 访客");

                list.Add(building);
            }

            return list;
        }

        /// <summary>
        /// 饭桌使用的仓库
        /// </summary>
        static Dictionary<int, Building> TableUsedStorages = new Dictionary<int, Building>();
        /// <summary>
        /// 移动至目标并设置目标占用
        /// </summary>
        /// <param name="_unit"></param>
        /// <param name="desires"></param>
        /// <param name="moveTarget"></param>
        /// <param name="__result"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        static bool MoveToTargetAndBindReservation(T_Citizen _unit, List<DesireInfo> desires, Building moveTarget, ref int __result, ref TileType type, bool isFood = true, Building searchTarget = null)
        {
            List<TileSt_Info> list = (searchTarget ?? moveTarget).List_TileObj;

            TileSt_Info info = null;

            foreach (DesireInfo desire in desires)
            {
                info = list.Find(t => t.m_Type == desire.NeedType);

                if (info != null)
                {
                    //Debug.Log($"{_unit.m_UnitName} 当前需求 {TileName(info.m_Type)}/{TileName(desires[0].NeedType)}");

                    break;
                }
            }

            int uid = _unit.m_ID;

            if (info != null && info.SetReservation(uid))
            {
                bool isTable = moveTarget.m_Info.Name == BuildingName.Table;

                CitizenState state = isTable ? CitizenState.UsingTable : isFood ? CitizenState.Eating : CitizenState.Using_Life;

                if (isTable)
                {
                    if (!TableUsedStorages.TryGetValue(uid, out Building tableStorage))
                        TableUsedStorages.Add(uid, searchTarget);
                    else
                        TableUsedStorages[uid] = searchTarget;
                }

                Debug.Log($"{_unit.m_UnitName} 准备前往 {moveTarget.m_CustomName} 进行 {state}");

                _unit.PathFindCall(new Vector3(moveTarget.Pos_Tile.x, moveTarget.Pos_Tile.y, moveTarget.m_ID), state, C_Key.CenterExam, false);

                type = info.m_Type;

                __result = 1;

                return true;
            }

            return false;
        }

        //[HarmonyPrefix, HarmonyPatch(typeof(T_Citizen), "PathFindCall")]
        //public static void T_Citizen_PathFindCall(T_Citizen __instance, Vector3 t_pos, CitizenState state)
        //{
        //    if (state == CitizenState.Eating || state == CitizenState.UsingTable)
        //    {
        //        //string name = __instance.m_TargetBuilding != null ? __instance.m_TargetBuilding.m_CustomName : "无";

        //        Debug.LogWarning($"{__instance.m_UnitName} 准备前往目标 {t_pos}，进行 {state}");
        //    }
        //}

        /// <summary>
        /// 寻找最近的建筑出售物品
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_unit"></param>
        /// <param name="_selltype"></param>
        /// <param name="_name"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool BuildingMgr_FindShortestBuilding_ForSell(BuildingMgr __instance, GameUnit _unit, TileType _selltype, BuildAbility _name, ref Building __result)
        {
            if (!ActiveOptimizeBuyAndSellPathFind)
                return true;

            Vector2 pos = _unit.m_CurNode.GetPos();

            List<Building> list = __instance.List_Building;

            if (_name == BuildAbility.Store)
            {
                List<Building_Storage> storages = __instance.List_Storage.Where(t => t.CanI_Add(_selltype, 1)).ToList();

                list = storages.Select(t => t as Building).ToList();
            }

            //Debug.LogWarning($"{_unit.m_UnitName} 出售 {_selltype}，共 {list.Count} 可选目标仓库");

            TryFindShortestBuilding(_unit, pos, list, out __result);

            return false;
        }

        /// <summary>
        /// 寻找仓库购买物品
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="pos"></param>
        /// <param name="_mat"></param>
        /// <param name="_unit"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool BuildingMgr_FindStorageForBuy(BuildingMgr __instance, Vector2 pos, TileType _mat, T_Citizen _unit, ref Building __result)
        {
            if (!ActiveOptimizeBuyAndSellPathFind)
                return true;

            List<Building_Storage> storages = __instance.List_Storage.Where(t => t.CanIBuy(_mat, _unit)).ToList();

            List<Building> list = storages.Select(t => t as Building).ToList();

            //Debug.LogWarning($"{_unit.m_UnitName} 购买 {_mat}，共 {list.Count} 可选目标仓库");

            TryFindShortestBuilding(_unit, pos, list, out __result);

            return false;
        }

        /// <summary>
        /// 寻找最近的建筑
        /// </summary>
        /// <param name="buildings"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        static bool TryFindShortestBuilding(GameUnit unit, Vector2 pos, List<Building> buildings, out Building result)
        {
            result = null;

            if (buildings.Count == 0)
                return false;

            List<Vector2> path, shortestPath = null;

            Vector2 endPos;

            bool isSearching = false;

            //Debug.Log($"\n开始计算目标路径，共 {buildings.Count} 目标");

            foreach (Building building in buildings)
            {
                endPos = TileMgr.GetTilePos(building.Pos_Tile);

                if (!TileMgr.CanI_StandBlock(endPos, 1))
                    continue;

                //原地或左右两格
                if (IsNearNode(pos, endPos, out _))
                {
                    result = building;

                    return true;
                }

                if (PathFindMgr.IsSearchingWay(pos, endPos))
                {
                    result = null;

                    Debug.Log($"{unit.m_UnitName} 存在未计算完成的路径目标！结束寻找！");

                    return false;
                }

                if (!TryGetPath(unit, pos, endPos, out path))
                {
                    if (PathFindMgr.IsSearchingWay(pos, endPos))
                    {
                        Debug.Log($"{unit.m_UnitName} 目标 {building.m_CustomName} 正在计算路径中...");

                        isSearching = true;
                    }
                    //else
                    //{
                    //    if (PathFindMgr.List_ClosedWay.Exists(t => t.IsTrackSame(pos, endPos)))
                    //        Debug.Log($"{unit.m_UnitName} 目标 {building.m_CustomName} 无法到达");
                    //    else if (!PathFindMgr.List_OpenWay.Exists(t => t.IsTrackSame(pos, endPos)))
                    //        Debug.LogWarning($"{unit.m_UnitName} 目标 {building.m_CustomName} 未获得路径，且未进行路径计算！");
                    //    else
                    //        Debug.Log($"{unit.m_UnitName} 目标 {building.m_CustomName} 已完成路径查找！");
                    //}

                    continue;
                }

                //string pathStr = string.Join(" -> ", path);

                if (shortestPath == null || path.Count < shortestPath.Count)
                {
                    shortestPath = path;

                    result = building;
                }

                //Debug.Log($"{unit.m_UnitName} 起点 {pos} 与目标 {building.m_CustomName}({building.Pos_Tile})({path.Count})");
            }

            if (isSearching)
            {
                result = null;

                Debug.Log($"{unit.m_UnitName} 正在计算目标路径，本次寻路结束\n");
            }
            //else if (result != null)
            //    Debug.LogWarningFormat($"{unit.m_UnitName} 起点 {pos} 与目标 {result.m_CustomName}({result.Pos_Tile}) 最近\n");

            return result != null;
        }

        /// <summary>
        /// 获得路径
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        static bool TryGetPath(GameUnit unit, Vector2 startPos, Vector2 endPos, out List<Vector2> path)
        {
            path = null;

            OpenWayInfo info;

            for (int i = 0; i < List_OpenWay.Count; i++)
            {
                info = List_OpenWay[i];

                if (GetMatchPath(info, startPos, endPos, out path))
                    return true;
            }

            List_ClosedWay.Clear();

            FindPathByMono(startPos, endPos, unit.GetMiniInfo(), null);

            //if (!List_ClosedWay.Exists(t => t.IsTrackSame(startPos, endPos)))
            //{
            //    //Debug.Log($"开始寻路，起点 {startPos} 终点 {endPos}");

            //    FindPathByMono(startPos, endPos, unit.GetMiniInfo(), null);
            //}
            //PathFindMgr.CanIpathFind2(endPos, startPos, unit.GetMiniInfo(), null);


            //Debug.LogError($"起点 {startPos} 终点 {endPos} 未获得路径 {TileMgr.CanI_StandBlock(endPos, 1)}");

            return path != null;
        }

        /// <summary>
        /// 获得相同路径
        /// </summary>
        /// <param name="info"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        static bool GetMatchPath(OpenWayInfo info, Vector2 startPos, Vector2 endPos, out List<Vector2> path)
        {
            path = null;

            if (info.IsTrackSame(startPos, endPos))
            {
                path = info.GetTrack(startPos, endPos);

                //Debug.Log($"起点 {startPos} 至终点 {endPos} 获得相同路径 {path.Count}");

                return true;
            }

            int offset;

            //目标与起点相邻
            if (info.Start_Area.Contains(endPos))
                info.List_Way.Reverse();
            //目标与终点相邻
            if (info.End_Area.Contains(endPos))
            {
                //获得路径上相邻的起点
                int index = info.List_Way.FindIndex(t => IsNearNode(startPos, t, out offset));

                //从起点开始到达终点
                if (index >= 0)
                {
                    path = info.List_Way.GetRange(index, info.List_Way.Count - index - 1);

                    if (path[0] != startPos)
                        path.Insert(0, startPos);
                    if (path[path.Count - 1] != endPos)
                        path.Add(endPos);

                    //Debug.Log($"起点 {startPos} 至终点 {endPos} 存在相似路径 {path.Count} / {info.List_Way.Count}");

                    return true;
                }
            }

            //自身与终点相邻
            if (info.End_Area.Contains(startPos))
                info.List_Way.Reverse();
            //自身与起点相邻
            if (info.Start_Area.Contains(startPos))
            {
                //获得路径上相邻的终点
                int index = info.List_Way.FindIndex(t => IsNearNode(endPos, t, out offset));

                //从终点开始到达起点
                if (index >= 0)
                {
                    path = info.List_Way.GetRange(0, index + 1);

                    if (path[0] != startPos)
                        path.Insert(0, startPos);
                    if (path[path.Count - 1] != endPos)
                        path.Add(endPos);

                    //Debug.Log($"起点 {startPos} 至终点 {endPos} 存在相似路径 {path.Count} / {info.List_Way.Count}");

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是相邻的格子
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        static bool IsNearNode(Vector2 pos, Vector2 target, out int offset)
        {
            bool sameY = pos.y == target.y;

            offset = sameY ? (int)Mathf.Abs(pos.x - target.x) : -1;

            //原地或左右两格
            return sameY && offset < 2;
        }

        static void FindPathByMono(Vector2 startPos, Vector2 targetPos, MiniUnitInfo mini_info, PathResult effect)
        {
            mono.StartCoroutine(((CustomMOD)mono).FindPathC(startPos, targetPos, mini_info, effect));
        }

        IEnumerator FindPathC(Vector2 startPos, Vector2 targetPos, MiniUnitInfo mini_info, PathResult effect)
        {
            FindPath(startPos, targetPos, mini_info, effect);

            //Debug.Log($"结束寻路 {startPos} {targetPos}");

            yield return 0;
        }

        /// <summary>
        /// 搜索列表
        /// </summary>
        static List<SearchingWayInfo> List_SearchWay { get { return PathFindMgr.List_SearchWay; } }
        static List<GameUnit> List_SearchingUnit { get { return PathFindMgr.List_SearchingUnit; } }
        /// <summary>
        /// 可移动路径
        /// </summary>
        static List<OpenWayInfo> List_OpenWay { get { return PathFindMgr.List_OpenWay; } }
        /// <summary>
        /// 不可移动路径
        /// </summary>
        static List<ClosedWayInfo> List_ClosedWay { get { return PathFindMgr.List_ClosedWay; } }
        /// <summary>
        /// 使用的key
        /// </summary>
        static List<int> List_UsingOpKey { get { return GetPrivateValue<List<int>>(PathFindMgr, "List_UsingOpKey"); } }
        /// <summary>
        /// 开放列表集
        /// </summary>
        static List<List<C_Node>> List_OpenLists { get { return GetPrivateValue<List<List<C_Node>>>(PathFindMgr, "List_OpenLists"); } }
        /// <summary>
        /// 最大迭代次数（非最大步长）
        /// </summary>
        static int LoopMaxNum { get { return GetPrivateValue<int>(PathFindMgr, "m_LoopMaxNum"); } }
        static void FindPath(Vector2 startPos, Vector2 targetPos, MiniUnitInfo mini_info, PathResult effect)
        {
            C_Node startNode = TileMgr.GetNode(startPos);
            C_Node targetNode = TileMgr.GetNode(targetPos);

            SearchingWayInfo s_info = new SearchingWayInfo(0, startNode.GetPos(), targetNode.GetPos());
            List_SearchWay.Add(s_info);
            List<C_Node> openList;
            int num;
            if (List_UsingOpKey.Count == 0)
            {
                openList = new List<C_Node>();
                List_OpenLists.Add(openList);
                num = List_OpenLists.Count - 1;
            }
            else
            {
                num = List_UsingOpKey[List_UsingOpKey.Count - 1];
                List_UsingOpKey.Remove(num);
                openList = List_OpenLists[num];
            }

            List<int> openKeys = new List<int>();
            List<Vector2> FinalNodeList = new List<Vector2>();
            openList.Clear();
            openList.Add(startNode);
            openKeys.Clear();
            int loopCount = 0;
            bool errorTarget = false;
            bool findPath = false;
            if (!C_Node.CanIOnBox(targetNode) || targetNode == null || targetNode.m_NodeType == NodeType.Wall)
            {
                errorTarget = true;
            }

            //Debug.Log($"准备计算 {startPos} 至 {targetPos}，{openList.Count} {LoopMaxNum} {errorTarget}");

            C_Node CurNode;
            while (openList.Count > 0 && loopCount < LoopMaxNum && !errorTarget)
            {
                loopCount++;
                CurNode = openList[0];
                int count = openList.Count;
                for (int i = 1; i < count; i++)
                {
                    //F（总代价）= G（开始点到当前方块的移动代价）+ H（当前方块到结束点的预估移动代价），H（曼哈顿距离算法） = x轴差值 + y轴差值 * 10
                    if (openList[i].F <= CurNode.F && openList[i].H < CurNode.H)
                    {
                        CurNode = openList[i];
                    }
                }

                int index = openList.FindIndex((C_Node temp) => temp.m_Key == CurNode.m_Key);
                openKeys.Add(openList[index].m_Key);
                openList.RemoveAt(index);
                if (CurNode.m_Key == targetNode.m_Key)
                {
                    errorTarget = true;
                    C_Node c_Node = CurNode;
                    while (c_Node != null && c_Node.m_Key != startNode.m_Key)
                    {
                        FinalNodeList.Add(c_Node.GetPos());
                        c_Node = c_Node.m_ParentNode;
                    }

                    findPath = true;
                    FinalNodeList.Add(startNode.GetPos());
                    FinalNodeList.Reverse();
                    if (!List_OpenWay.Exists((OpenWayInfo x) => x.IsSame(FinalNodeList)))
                    {
                        if (List_OpenWay.Count >= 500)
                        {
                            List_OpenWay.Clear();
                        }

                        List_OpenWay.Add(new OpenWayInfo(FinalNodeList, mini_info));
                    }

                    effect?.Invoke(value: false);

                    //Debug.Log($"路径 {startPos} 至 {targetPos} 寻找成功！共 {FinalNodeList.Count} 路径");
                    continue;
                }

                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, 1, 1, MoveDir.Right_Up);
                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, -1, 1, MoveDir.Left_Up);
                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, -1, -1, MoveDir.Left_Down);
                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, 1, -1, MoveDir.Right_Down);
                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, 2, 0, MoveDir.Right_Jump);
                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, -2, 0, MoveDir.Left_Jump);

                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, 0, 1, MoveDir.Up);
                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, 0, -1, MoveDir.Down);
                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, 1, 0, MoveDir.Right);
                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, -1, 0, MoveDir.Left);

                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, 1, 2, MoveDir.Right_Grab);
                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, -1, 2, MoveDir.Left_Grab);
                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, 1, -2, MoveDir.Right_Drop);
                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, -1, 2, MoveDir.Left_Drop);

                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, 2, 1, MoveDir.Right_UpJump);
                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, -2, 1, MoveDir.Left_UpJump);
                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, 2, -1, MoveDir.Right_DownJump);
                AddOpenList(CurNode, targetNode, openKeys, mini_info, num, -2, -1, MoveDir.Left_DownJump);
            }

            if (!findPath)
            {
                //if (List_ClosedWay.Count >= 250)
                //{
                //    List_ClosedWay.Clear();
                //}

                //Debug.LogWarning($"路径 {startPos} 至 {targetPos} 寻找失败！");

                //List_ClosedWay.Add(new ClosedWayInfo(startPos, targetPos));
                effect?.Invoke(value: true);
            }

            List_SearchWay.RemoveAll((SearchingWayInfo x) => x.IsSameTrack(s_info.m_StartPos, s_info.m_EndPos));
        }

        //static void FindPath(Vector3 startPos, Vector3 targetPos, GameUnit _unit/*, CitizenState state = CitizenState.WayTest*/, bool _IsReverse = false/*, C_Key _key = C_Key.None*/)
        //{
        //    C_Node S_Node = TileMgr.GetNode(startPos);
        //    C_Node T_Node = TileMgr.GetNode(targetPos);
        //    SearchingWayInfo s_info = new SearchingWayInfo(_unit.m_ID, S_Node.GetPos(), T_Node.GetPos());
        //    List_SearchWay.Add(s_info);
        //    if (!List_SearchingUnit.Contains(_unit))
        //    {
        //        List_SearchingUnit.Add(_unit);
        //    }

        //    if (_IsReverse)
        //    {
        //        C_Node c_Node = S_Node;
        //        S_Node = T_Node;
        //        T_Node = c_Node;
        //    }

        //    List<C_Node> OpenList;
        //    int Op_Index;
        //    if (List_UsingOpKey.Count == 0)
        //    {
        //        OpenList = new List<C_Node>();
        //        List_OpenLists.Add(OpenList);
        //        Op_Index = List_OpenLists.Count - 1;
        //    }
        //    else
        //    {
        //        Op_Index = List_UsingOpKey[List_UsingOpKey.Count - 1];
        //        List_UsingOpKey.Remove(Op_Index);
        //        OpenList = List_OpenLists[Op_Index];
        //    }

        //    List<int> ClosedList = new List<int>();
        //    OpenList.Clear();
        //    OpenList.Add(S_Node);
        //    ClosedList.Clear();
        //    List<Vector2> FinalNodeList = new List<Vector2>();
        //    MiniUnitInfo mini_info = _unit.GetMiniInfo();
        //    int temp_ChangeNum = GameMgr.Instance._SttMgr.m_ChangeTime;
        //    int loop_num = 0;
        //    bool loop_end = false;
        //    bool IsSuccess = false;
        //    bool IsWayCheckFinish = true;
        //    if (!_IsReverse)
        //    {
        //        if (!TileMgr.CanI_StandBlock(T_Node.GetPos()))
        //        {
        //            loop_end = true;
        //        }
        //    }
        //    else if (!TileMgr.CanI_StandBlock(S_Node.GetPos()))
        //    {
        //        loop_end = true;
        //    }

        //    int loop_max = LoopMaxNum;
        //    //if (_key == C_Key.LostHome && _unit.m_UnitKind == UnitKind.Queen)
        //    //{
        //    //    loop_max = 250;
        //    //}

        //    C_Node CurNode;
        //    while (OpenList.Count > 0 && loop_num < loop_max && !loop_end)
        //    {
        //        int num = loop_num + 1;
        //        loop_num = num;
        //        CurNode = OpenList[0];
        //        int count = OpenList.Count;
        //        for (int i = 1; i < count; i++)
        //        {
        //            if (OpenList[i].F <= CurNode.F && OpenList[i].H < CurNode.H)
        //            {
        //                CurNode = OpenList[i];
        //            }
        //        }

        //        int index = OpenList.FindIndex((C_Node temp) => temp.m_Key == CurNode.m_Key);
        //        ClosedList.Add(OpenList[index].m_Key);
        //        OpenList.RemoveAt(index);
        //        if (CurNode.m_Key == T_Node.m_Key)
        //        {
        //            loop_end = true;
        //            IsSuccess = true;
        //            C_Node c_Node2 = CurNode;
        //            while (c_Node2 != null && c_Node2.m_Key != S_Node.m_Key)
        //            {
        //                FinalNodeList.Add(c_Node2.GetPos());
        //                c_Node2 = c_Node2.m_ParentNode;
        //            }

        //            FinalNodeList.Add(S_Node.GetPos());
        //            if (!_IsReverse)
        //            {
        //                FinalNodeList.Reverse();
        //            }

        //            //if (state != CitizenState.WayTest && !_unit.List_TargetPos.Contains(targetPos))
        //            //{
        //            //    _unit.List_TargetPos.Add(targetPos);
        //            //}

        //            if (!List_OpenWay.Exists((OpenWayInfo x) => x.IsSame(FinalNodeList)))
        //            {
        //                if (List_OpenWay.Count >= 500)
        //                {
        //                    List_OpenWay.Clear();
        //                }

        //                List_OpenWay.Add(new OpenWayInfo(FinalNodeList, mini_info));
        //            }

        //            //if (_unit.PathFindCallBack(targetPos, IsSuccess: true, state, _key) && state != CitizenState.WayTest)
        //            //{
        //            //    _unit.Move(FinalNodeList);
        //            //    if (state != CitizenState.Nothing && !_unit.List_State.Contains(state))
        //            //    {
        //            //        _unit.AddState(state);
        //            //    }
        //            //}
        //        }
        //        else
        //        {
        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, 1, 1, MoveDir.Right_Up);
        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, -1, 1, MoveDir.Left_Up);
        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, -1, -1, MoveDir.Left_Down);
        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, 1, -1, MoveDir.Right_Down);
        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, 2, 0, MoveDir.Right_Jump);
        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, -2, 0, MoveDir.Left_Jump);

        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, 0, 1, MoveDir.Up);
        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, 0, -1, MoveDir.Down);
        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, 1, 0, MoveDir.Right);
        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, -1, 0, MoveDir.Left);

        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, 1, 2, MoveDir.Right_Grab);
        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, -1, 2, MoveDir.Left_Grab);
        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, 1, -2, MoveDir.Right_Drop);
        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, -1, 2, MoveDir.Left_Drop);

        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, 2, 1, MoveDir.Right_UpJump);
        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, -2, 1, MoveDir.Left_UpJump);
        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, 2, -1, MoveDir.Right_DownJump);
        //            AddOpenList(CurNode, T_Node, ClosedList, mini_info, Op_Index, -2, -1, MoveDir.Left_DownJump);
        //        }

        //        if (!loop_end && loop_num >= PathFindMgr.m_PathShortCheckNum)
        //        {
        //            if (loop_num < 300 && loop_num % 2 == 0)
        //            {
        //                return;
        //            }
        //            else if (loop_num >= 300)
        //            {
        //                return;
        //            }

        //            if (temp_ChangeNum != GameMgr.Instance._SttMgr.m_ChangeTime)
        //            {
        //                loop_end = true;
        //                IsWayCheckFinish = false;
        //            }
        //            else if ((loop_num == 300 || loop_num == 350) && !List_SearchWay.Contains(s_info))
        //            {
        //                loop_end = true;
        //                IsWayCheckFinish = false;
        //            }
        //        }
        //    }

        //    if (IsWayCheckFinish && !IsSuccess)
        //    {
        //        if (S_Node.GetPos() == T_Node.GetPos())
        //        {
        //            IsSuccess = true;
        //            //_unit.PathFindCallBack(targetPos, IsSuccess: true, state, _key);
        //            //if (state != CitizenState.WayTest && state != CitizenState.Nothing && !_unit.List_State.Contains(state))
        //            //{
        //            //    _unit.List_State.Add(state);
        //            //}
        //        }
        //        //else
        //        //{
        //        //    if (List_ClosedWay.Count >= 250)
        //        //    {
        //        //        List_ClosedWay.Clear();
        //        //    }

        //        //    List_ClosedWay.Add(new ClosedWayInfo(startPos, targetPos));
        //        //    //_unit.PathFindCallBack(targetPos, IsSuccess: false, state, _key);
        //        //}

        //        //if (!IsSuccess && _unit.m_LostNum >= 5 && _unit.m_UnitKind == UnitKind.Citizen && Random.Range(0, 5) == 0)
        //        //{
        //        //    LogX.Log("Lost Way UnitName: " + _unit.m_UnitName + " / State : " + state.ToString() + " / S_Pos : " + S_Node.GetPos().ToString() + " / E_Pos : " + T_Node.GetPos().ToString());
        //        //    (_unit as T_Citizen)?.ReRate_FreeFunc(state);
        //        //}
        //    }

        //    if (List_SearchingUnit.Contains(_unit))
        //    {
        //        List_SearchingUnit.Remove(_unit);
        //    }

        //    List_SearchWay.RemoveAll((SearchingWayInfo x) => x.IsSameTrack(s_info.m_StartPos, s_info.m_EndPos));
        //    List_UsingOpKey.Add(Op_Index);
        //}

        /// <summary>
        /// 添加进开放列表
        /// </summary>
        /// <param name="CurNode"></param>
        /// <param name="node2"></param>
        /// <param name="list"></param>
        /// <param name="mini_info"></param>
        /// <param name="num"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="moveDir"></param>
        static void AddOpenList(C_Node CurNode, C_Node node2, List<int> list, MiniUnitInfo mini_info, int num, int x, int y, MoveDir moveDir)
        {
            C_Node MoveNode = TileMgr.GetNode(CurNode.x + x, CurNode.y + y);

            if (MoveNode != null && !list.Exists((int temp) => temp == MoveNode.m_Key))
            {
                Traverse.Create(PathFindMgr).Method("OpenListAdd", CurNode, node2, MoveNode, moveDir, mini_info, num).GetValue();
            }
        }

        #endregion

        #region 女王建造拆除不丢弃物品

        /// <summary>
        /// 女王不丢弃物品
        /// </summary>
        static bool NoDropFromQueen = false;
        /// <summary>
        /// 女王丢弃物品前
        /// 来源为建造事件时，标记跳过丢弃
        /// </summary>
        public static void T_Queen_DropAction(T_Queen __instance)
        {
            if (!ActiveActionNoDropGatheredItems)
                return;

            //Debug.Log($"女王丢弃物品前 {__instance.m_CharState}");

            bool inIdleInteraction = InputMgr.GetKeyDown(HotKeyName.Interaction) && (__instance.m_CharState == CharState.None || __instance.m_CharState == CharState.Carrying) && __instance.m_AniState == AniState.Idle;

            bool whiteTargetList = __instance.m_CheckBox.IsBP_ConstructEnable() || __instance.m_CheckBox.IsBP_PlantConstructEnable();

            //符合目标状态交互时
            if (whiteTargetList && inIdleInteraction)
            {
                NoDropFromQueen = true;

                //Debug.Log("女王触发不丢弃物品");
            }
        }

        /// <summary>
        /// 丢弃拾取的物品
        /// 根据标记跳过执行
        /// </summary>
        /// <returns></returns>
        public static bool GameUnit_Drop_GatheringList(GameUnit __instance)
        {
            if (__instance.List_Gathering.Count == 0)
                return true;

            //string buildName = __instance.m_BP_Builidng != null ? $" {__instance.m_BP_Builidng.m_Info.T_Name}" : "无";

            //Debug.Log($"{__instance.m_UnitName} 丢弃 {__instance.List_Gathering[0].m_Type}，当前状态 {__instance.m_CharState} / {__instance.m_AniState}，目标建筑 {buildName}，不丢弃物品 {NoDropFromQueen}({!ActiveActionNoDropGatheredItems || !NoDropFromQueen})");

            return !ActiveActionNoDropGatheredItems || !NoDropFromQueen;
        }

        /// <summary>
        /// 女王重置状态
        /// Update的最后会执行该方法进行状态重置，这里重置标记
        /// </summary>
        public static void T_Queen_BehaviorStop(T_Queen __instance)
        {
            if (!ActiveActionNoDropGatheredItems || __instance.List_Gathering.Count == 0 || (!NoDropFromQueen && !DoingDemolition))
                return;

            SetQueenCarryingState(__instance);
        }

        static void SetQueenCarryingState(T_Queen __instance)
        {
            Debug.Log($"女王操作结束，返回携带状态 {__instance.m_CharState} => {CharState.Carrying}，剩余资源 {__instance.List_Gathering.Count}");

            __instance.SetCharState(CharState.Carrying);

            //动画会根据状态执行分类动画
            __instance.SetAniState(AniState.Idle, true, false);

            __instance.m_CheckBox.HotKeyUpdate();

            NoDropFromQueen = false;

            DoingDemolition = false;
        }

        ///// <summary>
        ///// 女王动画
        ///// 这里控制从建造返回待机时的动画
        ///// </summary>
        //[HarmonyPrefix, HarmonyPatch(typeof(T_Queen), "SetAniState")]
        //public static void T_Queen_SetAniState(T_Queen __instance, AniState _state)
        //{
        //    if (!ActiveActionNoDropGatheredItems)
        //        return;

        //    Debug.Log($"女王当前动画 {__instance.m_AniState} => {_state} / {__instance.m_CharState}，拾取 {__instance.List_Gathering.Count}，不丢弃物品 {NoDropFromQueen}");

        //    //从建造动画返回待机动画时
        //    if (NoDropFromQueen && __instance.m_AniState == AniState.Building && __instance.List_Gathering.Count > 0)
        //    {
        //        __instance.SetCharState(CharState.Carrying);

        //        __instance.m_CheckBox.HotKeyUpdate();

        //        Debug.LogWarning($"女王返回携带状态 {__instance.m_CharState}，携带物品为 {__instance.List_Gathering[0].m_Type}");
        //    }
        //}

        /// <summary>
        /// 拆除启用状态
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool QueenCheckBox_IsDemolitionEnable(QueenCheckBox __instance, ref int __result)
        {
            if (!ActiveActionNoDropGatheredItems || GetPrivateValue<T_Queen>(__instance, "m_Queen").List_Gathering.Count == 0)
                return true;

            __result = 0;

            if (__instance.m_Building != null && __instance.AreYouSelectType(MiniType.Building) && GameMgr.Instance._SysMgr.List_DemolitionMark.Exists((WorkMark x) => x.m_Building == __instance.m_Building))
            {
                __result = 1;
            }

            if (__instance.m_Tile != null && __instance.m_Tile.IsBuilding && __instance.m_Tile.m_HP > 0f && __instance.AreYouSelectType(MiniType.Tile) && GameMgr.Instance._SysMgr.List_DemolitionMark.Exists((WorkMark x) => x.m_Tile == __instance.m_Tile))
            {
                __result = 2;
            }

            return false;

            //Debug.Log($"女王建造结束，返回携带状态 {__instance.m_CharState}，剩余资源 {__instance.List_Gathering.Count}");
        }

        static bool StartDemolition = false;
        /// <summary>
        /// 执行拆除中
        /// </summary>
        static bool DoingDemolition = false;
        /// <summary>
        /// 女王Update监听
        /// </summary>
        /// <param name="__instance"></param>
        public static void T_Queen_Update(T_Queen __instance)
        {
            if (!ActiveActionNoDropGatheredItems || __instance.List_Gathering.Count == 0)
                return;

            if (GameMgr.Instance._TileMgr.IsSandBoxMode || GameMgr.Instance._SysMgr.IsGamePause() || Time.timeScale == 0f || __instance.m_AlivePause)
            {
                return;
            }

            if (__instance.m_CheckBox.IsDemolitionEnable() > 0)
            {
                if (InputMgr.GetKeyDown(HotKeyName.Interaction) && (__instance.m_CharState == CharState.None || __instance.m_CharState == CharState.Carrying) && __instance.m_AniState == AniState.Idle && __instance.m_CheckBox.IsDemolitionEnable() == 1)
                {
                    Debug.Log($"女王开始拆除1，携带 {__instance.List_Gathering.Count} 物品");

                    if (__instance.m_CheckBox.m_Building.m_Unit != null && __instance.m_CheckBox.m_Building.m_Unit != __instance && __instance.m_CheckBox.m_Building.m_Unit.m_UnitKind == UnitKind.Citizen)
                    {
                        __instance.m_CheckBox.m_Building.m_Unit.GetComponent<T_Citizen>().GaugeVarCalculate(TypeOrder.Demolition);
                        __instance.m_CheckBox.m_Building.m_Unit.GetComponent<T_Citizen>().StateReset();
                    }

                    if (DefinesGetValue<bool>("IsMouseTargetOn"))
                    {
                        __instance.OnlyFlipX(__instance.m_CheckBox.m_Building.Tf.position.x > __instance.Tf.position.x);
                    }

                    DoingDemolition = true;
                    __instance.m_CheckBox.m_Building.m_Unit = __instance;
                    __instance.SetCharState(CharState.Building);
                    __instance.m_CheckBox.m_Building.m_Gauge.gameObject.SetActive(value: true);
                    __instance.SetAniState(AniState.Building, _loop: true, _now: false);
                    return;
                }

                if (InputMgr.GetKeyDown(HotKeyName.Interaction) && __instance.m_CheckBox.IsInfoBoxOn() && __instance.m_CheckBox.IsDemolitionEnable() == 2 && __instance.m_CheckBox.m_Tile.IsBuilding && GameMgr.Instance._SysMgr.List_DemolitionMark.Exists((WorkMark x) => x.m_Tile == __instance.m_CheckBox.m_Tile))
                {
                    Debug.Log($"女王开始拆除2，携带 {__instance.List_Gathering.Count} 物品");

                    if (__instance.m_CheckBox.m_Tile.m_Unit != null && __instance.m_CheckBox.m_Tile.m_Unit != __instance && __instance.m_CheckBox.m_Tile.m_Unit.m_UnitKind == UnitKind.Citizen)
                    {
                        __instance.m_CheckBox.m_Tile.m_Unit.GetComponent<T_Citizen>().GaugeVarCalculate(TypeOrder.Demolition);
                        __instance.m_CheckBox.m_Tile.m_Unit.GetComponent<T_Citizen>().StateReset();
                    }

                    __instance.SetCharState(CharState.Building);
                    if (DefinesGetValue<bool>("IsMouseTargetOn"))
                    {
                        __instance.OnlyFlipX(__instance.m_CheckBox.m_Tile.Tf.position.x > __instance.Tf.position.x);
                    }

                    StartDemolition = true;
                    DoingDemolition = true;
                    __instance.Kill_MoveCoroutine(GameUnit.Kill_MoveState.Only_corMoving);
                    __instance.m_CheckBox.m_Tile.m_Unit = __instance;
                    __instance.m_WorkTimeGauge.GaugeActive(__instance.m_CheckBox.m_Tile, DemolitionEffect);
                    __instance.SetAniState(AniState.Building, _loop: true, _now: false);
                    return;
                }
            }
            else if (StartDemolition)
            {
                StartDemolition = false;

                SetQueenCarryingState(__instance);
            }

            void RepairEffect()
            {
                __instance.SetCharState(CharState.None);
                __instance.m_WorkTimeGauge.StopGauge(TypeOrder.Repairing);
                __instance.SetAniState(AniState.Idle, _loop: true, _now: false);
            }

            void DemolitionEffect()
            {
                if (__instance.m_CheckBox.m_Tile != null && __instance.m_CheckBox.m_Tile.IsBuilding)
                {
                    __instance.m_CheckBox.m_Tile.DestroyTile(Dir_check: true);
                }

                __instance.SetCharState(CharState.None);
                __instance.m_WorkTimeGauge.StopGauge(TypeOrder.Demolition);
                __instance.SetAniState(AniState.Idle, _loop: true, _now: false);
            }

            //Debug.Log($"女王建造结束，返回携带状态 {__instance.m_CharState}，剩余资源 {__instance.List_Gathering.Count}");
        }

        #endregion

        #region 女王Update逻辑解析

        private void QueenUpdate(T_Queen __instance)
        {
            if (__instance.m_CommandMode)
            {
                //m_Spine.Update(Time.unscaledDeltaTime);
            }

            if (__instance.m_CharState == CharState.Death)
            {
                return;
            }

            if (__instance.m_CurNode != null)
            {
                GameMgr.Instance._MinimapUI.PosUpdate(__instance.m_CurNode.GetIntPos());
            }

            //Update_Wheel();
            if (GameMgr.Instance._TileMgr.IsSandBoxMode || GameMgr.Instance._SysMgr.IsGamePause() || Time.timeScale == 0f || __instance.m_AlivePause)
            {
                return;
            }

            //站立时，检测脚下的方块状态
            if (__instance.m_AniState != AniState.Jump && __instance.m_AniState != AniState.DashJump && __instance.m_AniState != AniState.ClimbJump && !__instance.IsAirWalk())
            {
                __instance.NodeUpdate();
                if (!C_Node.CanI_InBox(__instance.m_CurNode))
                {
                    __instance.ActionCancel();
                    //Atk_Init();
                    C_Node c_Node = null;
                    int num = __instance.m_CurNode.y - 1;
                    while (num >= 0 && (c_Node == null || !C_Node.CanIOnBox(c_Node)))
                    {
                        c_Node = GameMgr.Instance._TileMgr.GetNode(__instance.m_CurNode.x, num);
                        num--;
                    }

                    if (c_Node != null)
                    {
                        __instance.Move(MoveDir.FallDown, GameMgr.Instance._TileMgr.GetNode(c_Node.x, c_Node.y));
                    }
                }

                __instance.m_CheckBox.UpdateAct(value: true);
            }
            else
            {
                __instance.m_CheckBox.UpdateAct(value: false);
            }

            __instance.CheckBoxUpdate();
            if (__instance._corKnockback.IsRunning)
            {
                return;
            }

            if (__instance.m_CharState == CharState.OnRail)
            {
                //if (RailMove())
                //{
                //    return;
                //}
            }
            else if (__instance.m_CharState == CharState.OnLift)
            {
                //if (LiftMove())
                //{
                //    return;
                //}
            }
            else
            {
                if (__instance.m_CharState == CharState.Attack)
                {
                    //Update_AtkCheck();
                    return;
                }

                if (__instance.m_CharState == CharState.Skill_Use)
                {
                    return;
                }

                if (__instance.m_CharState == CharState.Queen_Action)
                {
                    if ((InputMgr.GetKey(HotKeyName.LeftDir) || InputMgr.GetKey(HotKeyName.RightDir)) && !__instance.IsThisAniRun("Alter_Action"))
                    {
                        __instance.ActionCancel();
                    }

                    return;
                }
            }

            if (InputMgr.GetKeyUp(HotKeyName.Screenshot))
            {
                if (!PlayDataMgr.Instance.IsSpaceOut)
                {
                    GameMenuMgr.Instance.ShowCapture();
                }
                else
                {
                    GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(UIUtility.GetTranslate("Alarm/Cant hotkey here"), Color.red);
                }

                return;
            }

            if (InputMgr.GetKeyUp(HotKeyName.UIToggle))
            {
                GameMgr.Instance._CamMgr.NextHUD();
            }

            //未移动且在工作、移动中时
            if ((InputMgr.GetKeyUp(HotKeyName.LeftDir) || InputMgr.GetKeyUp(HotKeyName.RightDir)) && !__instance.IsAirWalk() && (__instance.m_AniState == AniState.Walking || __instance.m_AniState == AniState.Dash || __instance.m_AniState == AniState.Falling_Walking))
            {
                __instance.KillMoving(_fall_check: false);
                __instance.SetAniState(AniState.Idle, _loop: true, _now: false);
                return;
            }
            
            //无目标选择
            if (!DefinesGetValue<bool>("IsMouseTargetOn"))
            {
                if (!__instance.IsAirWalk() && !InputMgr.GetKey(HotKeyName.UpDir) && !InputMgr.GetKey(HotKeyName.BottomDir) && !InputMgr.GetKey(HotKeyName.LeftDir) && !InputMgr.GetKey(HotKeyName.RightDir))
                {
                    if (InputMgr.GetKeyDown(HotKeyName.UpDir2))
                    {
                        __instance.m_CheckBox.CenterUpdate(0, 1);
                        __instance.CheckBoxUpdate();
                        __instance.m_CheckBox.HotKeyUpdate();
                        return;
                    }

                    if (InputMgr.GetKeyDown(HotKeyName.BottomDir2))
                    {
                        __instance.m_CheckBox.CenterUpdate(0, -1);
                        __instance.CheckBoxUpdate();
                        __instance.m_CheckBox.HotKeyUpdate();
                        return;
                    }

                    if (InputMgr.GetKeyDown(HotKeyName.LeftDir2))
                    {
                        __instance.m_CheckBox.CenterUpdate(-1, 0);
                        __instance.CheckBoxUpdate();
                        __instance.m_CheckBox.HotKeyUpdate();
                        return;
                    }

                    if (InputMgr.GetKeyDown(HotKeyName.RightDir2))
                    {
                        __instance.m_CheckBox.CenterUpdate(1, 0);
                        __instance.CheckBoxUpdate();
                        __instance.m_CheckBox.HotKeyUpdate();
                        return;
                    }
                }
            }
            //非挖矿、采集、建造，更新鼠标碰撞中心
            else if (!__instance.IsAirWalk() && __instance.m_CharState != CharState.Mining && __instance.m_CharState != CharState.Gathering && __instance.m_CharState != CharState.Building)
            {
                if (Cursor.visible)
                {
                    Vector2Int vector2Int = GameMgr.Instance._SysMgr.FindQueenTargetPos(__instance.m_CurNode.GetIntPos(), GameMgr.Instance._CamMgr.GetMousePos()) - __instance.m_CurNode.GetIntPos();
                    if (__instance.m_CheckBox.Pos_Center != vector2Int)
                    {
                        __instance.m_CheckBox.CenterUpdate(vector2Int);
                    }
                }

                //m_LastMousePos = GameMgr.Instance._CamMgr.GetMousePos();
            }

            if (InputMgr.GetKeyUp(HotKeyName.WeaponOn))
            {
                __instance.WeaponAct(_isfirst: true);
                return;
            }

            //pad菜单1
            if (InputMgr.ControlScheme == ControlScheme.Gamepad)
            {
                if (!__instance.IsAirWalk() && !__instance.IsFalling() && InputMgr.GetKeyUp(HotKeyName.Command))
                {
                    //if (!UseZoom)
                    //{
                    //    Open_OrderSlot();
                    //}
                    //else
                    //{
                    //    UseZoom = false;
                    //}

                    return;
                }
            }
            //菜单1
            else if (!__instance.IsAirWalk() && !__instance.IsFalling() && InputMgr.GetKeyUp(HotKeyName.Command))
            {
                __instance.Open_OrderSlot();
                /*UseZoom = false*/;
                return;
            }
            
            //菜单2
            if (!__instance.IsAirWalk() && !__instance.IsFalling() && InputMgr.GetKeyUp_F(HotKeyName.Command2))
            {
                __instance.Open_BottomCommandSlot();
            }
            //主逻辑
            else
            {
                //if (Update_AtkCheck() || (IsSpaceOut == 0 && (Update_EventUnitCheck() || Update_TaxCollectingCheck())) || Update_SkillCheck() || Update_ShortcutCheck())
                //{
                //    return;
                //}

                //女王建筑交互：待机动画且在携带或初始状态下
                if (InputMgr.GetKeyDown(HotKeyName.Interaction) && __instance.m_AniState == AniState.Idle && (__instance.m_CharState == CharState.None || __instance.m_CharState == CharState.Carrying) && !__instance._corSkill.IsRunning && !GameMgr.Instance._SysMgr.IsGamePause())
                {
                    if (__instance.m_CheckBox.AreYouSelectMapObjType(MapObjName.LordGreed) && !GameMgr.Instance._BatchUI.m_BatchFrameCoolTime)
                    {
                        if (GameMgr.Instance._MapObjMgr.m_LordGreed != null && GameMgr.Instance._MapObjMgr.m_LordGreed.Obj_SpeechBubble.activeSelf)
                        {
                            GameMgr.Instance._MapObjMgr.m_LordGreed.FeedSomething(__instance);
                        }

                        return;
                    }

                    if (__instance.m_CheckBox.AreYouSelectMapObjType(MapObjName.MagicianGrave) && !GameMgr.Instance._BatchUI.m_BatchFrameCoolTime)
                    {
                        MapObj selectMapObj = __instance.m_CheckBox.GetSelectMapObj();
                        if (selectMapObj != null && selectMapObj.m_Info.m_Name == MapObjName.MagicianGrave)
                        {
                            (selectMapObj as MagicianGrave).ReturnBuff(__instance);
                        }

                        return;
                    }

                    if (__instance.m_CheckBox.AreYouSelectMapObjType(MapObjName.QuestBird) && !GameMgr.Instance._BatchUI.m_BatchFrameCoolTime)
                    {
                        MapObj selectMapObj2 = __instance.m_CheckBox.GetSelectMapObj();
                        if (selectMapObj2 != null && selectMapObj2.m_Info.m_Name == MapObjName.QuestBird)
                        {
                            selectMapObj2.SetReadyAction();
                        }

                        return;
                    }
                }

                //隧道
                if (InputMgr.GetKeyDown(HotKeyName.Interaction) && __instance.m_AniState == AniState.Idle && (__instance.m_CharState == CharState.None || __instance.m_CharState == CharState.Carrying) && !__instance._corSkill.IsRunning && !GameMgr.Instance._SysMgr.IsGamePause() && __instance.m_CheckBox.IsBuildInfoOn() && !GameMgr.Instance._BatchUI.m_BatchFrameCoolTime && __instance.m_CheckBox.m_Building != null && __instance.m_CheckBox.m_Building.m_Info.Ability == BuildAbility.Tunnel && __instance.m_CheckBox.m_Building.m_BuildState == BuildState.Basic && !__instance.m_CheckBox.m_Building.IsCanRepair() && !GameMgr.Instance._SysMgr.IsRepariOrDemolitionMark(__instance.m_CheckBox.m_Building))
                {
                    __instance.TunnelActCall(__instance.m_CheckBox.m_Building);
                    return;
                }

                //望远镜
                if (InputMgr.GetKeyDown(HotKeyName.Interaction) && __instance.m_AniState == AniState.Idle && __instance.m_CharState == CharState.None && !__instance._corSkill.IsRunning && !GameMgr.Instance._SysMgr.IsGamePause() && __instance.m_CheckBox.IsBuildInfoOn() && !GameMgr.Instance._BatchUI.m_BatchFrameCoolTime && __instance.m_CheckBox.m_Building != null && __instance.m_CheckBox.m_Building.m_Info.Name == BuildingName.Telescope && __instance.m_CheckBox.m_Building.m_BuildState == BuildState.Basic && !__instance.m_CheckBox.m_Building.IsCanRepair() && !GameMgr.Instance._SysMgr.IsRepariOrDemolitionMark(__instance.m_CheckBox.m_Building))
                {
                    __instance.TelescopeInteraction(__instance.m_CheckBox.m_Building);
                    return;
                }

                //女王椅
                if (InputMgr.GetKeyDown(HotKeyName.Interaction) && __instance.m_AniState == AniState.Idle && __instance.m_CharState == CharState.None && !__instance._corSkill.IsRunning && !GameMgr.Instance._SysMgr.IsGamePause() && __instance.m_CheckBox.IsBuildInfoOn() && !GameMgr.Instance._BatchUI.m_BatchFrameCoolTime && __instance.m_CheckBox.m_Building != null && __instance.m_CheckBox.m_Building.m_Info.Ability == BuildAbility.Deco_QueenChair && __instance.m_CheckBox.m_Building.m_BuildState == BuildState.Basic && !__instance.m_CheckBox.m_Building.IsCanRepair() && !GameMgr.Instance._SysMgr.IsRepariOrDemolitionMark(__instance.m_CheckBox.m_Building))
                {
                    __instance.QueenChairInteraction(__instance.m_CheckBox.m_Building);
                    return;
                }

                //钟楼
                if (InputMgr.GetKeyDown(HotKeyName.Interaction) && __instance.m_AniState == AniState.Idle && __instance.m_CharState == CharState.None && !__instance._corSkill.IsRunning && !GameMgr.Instance._SysMgr.IsGamePause() && __instance.m_CheckBox.IsBuildInfoOn() && !GameMgr.Instance._BatchUI.m_BatchFrameCoolTime && __instance.m_CheckBox.m_Building != null && __instance.m_CheckBox.m_Building.m_Info.Name == BuildingName.BellTower && __instance.m_CheckBox.m_Building.m_BuildState == BuildState.Basic && !__instance.m_CheckBox.m_Building.IsCanRepair() && !GameMgr.Instance._SysMgr.IsRepariOrDemolitionMark(__instance.m_CheckBox.m_Building))
                {
                    __instance.BellInteraction(__instance.m_CheckBox.m_Building);
                    return;
                }

                //女王床
                if (InputMgr.GetKeyDown(HotKeyName.Interaction) && __instance.m_AniState == AniState.Idle && __instance.m_CharState == CharState.None && !__instance._corSkill.IsRunning && !GameMgr.Instance._SysMgr.IsGamePause() && __instance.m_CheckBox.IsBuildInfoOn() && !GameMgr.Instance._BatchUI.m_BatchFrameCoolTime && __instance.m_CheckBox.m_Building != null && __instance.m_CheckBox.m_Building.m_Info.Ability == BuildAbility.QueenBed && __instance.m_CheckBox.m_Building.m_BuildState == BuildState.Basic && !__instance.m_CheckBox.m_Building.IsCanRepair() && !GameMgr.Instance._SysMgr.IsRepariOrDemolitionMark(__instance.m_CheckBox.m_Building))
                {
                    __instance.BedInteraction(__instance.m_CheckBox.m_Building);
                    return;
                }

                //铁路
                if (InputMgr.GetKeyDown(HotKeyName.Interaction) && __instance.m_AniState == AniState.Idle && (__instance.m_CharState == CharState.None || __instance.m_CharState == CharState.Carrying) && !GameMgr.Instance._SysMgr.IsGamePause() && __instance.m_CheckBox.IsBuildInfoOn() && !GameMgr.Instance._BatchUI.m_BatchFrameCoolTime && __instance.m_CheckBox.m_Building != null && __instance.m_CheckBox.m_Building.m_Info.Ability == BuildAbility.Rail && __instance.m_CheckBox.m_Building.m_BuildState == BuildState.Basic && !GameMgr.Instance._SysMgr.IsRepariOrDemolitionMark(__instance.m_CheckBox.m_Building.Pos_Tile) && !__instance.m_CheckBox.m_Building.IsCanRepair())
                {
                    __instance.NodeUpdate();
                    if (__instance.m_CurNode.m_TileType == TileType.Railroad)
                    {
                        __instance.m_CheckBox.gameObject.SetActive(value: false);
                        __instance.m_Addmop.SetActivate(UnitBodyAddmop.Addmop.Train, _act: true);
                        __instance.SetCharState(CharState.OnRail);
                        __instance.m_RunningEffect.gameObject.SetActive(value: false);
                        __instance.SetAniState(AniState.Idle, _loop: true, _now: true);
                    }
                    else
                    {
                        GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Distance too far"), Color.red);
                    }

                    return;
                }

                //上下移动时
                if ((InputMgr.GetKey(HotKeyName.UpDir) || InputMgr.GetKey(HotKeyName.BottomDir)) && (__instance.m_AniState == AniState.Idle || __instance.m_AniState == AniState.Dash) && (__instance.m_CharState == CharState.None || __instance.m_CharState == CharState.Carrying) && !GameMgr.Instance._SysMgr.IsGamePause())
                {
                    __instance.NodeUpdate();
                    if (__instance.m_CurNode.m_BuildType == BuildType.SpecialBuildObj)
                    {
                        Vector2Int c_pos = __instance.m_CurNode.GetIntPos();
                        Building building = GameMgr.Instance._BuildingMgr.List_LiftPlatform.Find((Building x) => x.List_BuildPos.Count >= 2 && x.List_BuildPos[1] == c_pos);
                        if (building != null && building.m_Info.Ability == BuildAbility.Lift && building.m_BuildState == BuildState.Basic)
                        {
                            __instance.Tf.position = new Vector3(building.List_BuildPos[1].x, building.List_BuildPos[1].y, __instance.Tf.position.z);
                            __instance.NodeUpdate();
                            __instance.m_CheckBox.gameObject.SetActive(value: false);
                            __instance.m_Addmop.SetActivate(UnitBodyAddmop.Addmop.Lift, _act: true);
                            __instance.m_Addmop.m_Lift.LiftPerSet(GameMgr.Instance._BuildingMgr.GetLiftPer(__instance.GetPos()));
                            __instance.SetCharState(CharState.OnLift);
                            __instance.m_RunningEffect.gameObject.SetActive(value: false);
                            __instance.SetAniState(AniState.Idle, _loop: true, _now: true);
                            return;
                        }
                    }
                }

                //建筑交互
                if (InputMgr.GetKeyDown(HotKeyName.Interaction) && __instance.m_AniState == AniState.Idle && __instance.m_CharState == CharState.Carrying && !GameMgr.Instance._SysMgr.IsGamePause() && __instance.m_CheckBox.IsBuildInfoOn() && !GameMgr.Instance._BatchUI.m_BatchFrameCoolTime && __instance.m_CheckBox.m_Building.m_BuildState == BuildState.Basic)
                {
                    //火葬场
                    if (__instance.List_Gathering.Count != 0 && (__instance.m_CheckBox.m_Building.m_Info.Name == BuildingName.Crematory || __instance.m_CheckBox.m_Building.m_Info.Name == BuildingName.ElectricCrematory))
                    {
                        if (__instance.m_CheckBox.m_Building.m_Activation)
                        {
                            __instance.m_CheckBox.m_Building.IsFunction2OK();
                            return;
                        }

                        GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Building not normal"), Color.red);
                    }

                    //机器人维修站？
                    if (__instance.List_Gathering.Count != 0 && __instance.List_Gathering.Exists((TileSt_Info x) => x != null && x.m_Type == TileType.GBot) && __instance.m_CheckBox.m_Building.m_Info.Ability == BuildAbility.RatronStation)
                    {
                        if (__instance.m_CheckBox.m_Building.m_BuildState != 0 || !__instance.m_CheckBox.m_Building.m_Activation)
                        {
                            GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Building not normal"), Color.red);
                        }
                        else if (__instance.m_CheckBox.m_Building.m_BuildInfoUI.IsGuestReady())
                        {
                            if (__instance.IsDischargeGBotGather())
                            {
                                int gatherUnitIndex = __instance.m_GatherUnitIndex;
                                __instance.DropAction();
                                GBot gBot = GameMgr.Instance._T_UnitMgr.FindGBot(gatherUnitIndex);
                                if (gBot != null)
                                {
                                    (__instance.m_CheckBox.m_Building as Building_RatronStation)?.GuestSet(gBot);
                                }
                            }
                        }
                        else
                        {
                            GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Someone use"), Color.red);
                        }

                        return;
                    }

                    //携带市民时
                    if (__instance.List_Gathering.Count != 0 && __instance.List_Gathering.Exists((TileSt_Info x) => x != null && x.m_Type == TileType.A_Citizen))
                    {
                        if (__instance.m_CheckBox.m_Building.m_Info.Ability == BuildAbility.Canopy)
                        {
                            if (__instance.m_CheckBox.m_Building.IsFunctionOK())
                            {
                                if (!__instance.IsInjuryCitizenGather())
                                {
                                    return;
                                }

                                int gatherUnitIndex2 = __instance.m_GatherUnitIndex;
                                __instance.DropAction();
                                T_Citizen t_Citizen = GameMgr.Instance._T_UnitMgr.FindCitizen(gatherUnitIndex2);
                                if (t_Citizen != null)
                                {
                                    (__instance.m_CheckBox.m_Building as Building_House).GuestSet(t_Citizen);
                                    __instance.m_CheckBox.m_Building.Building_Update2(2f);
                                    AudioController.PlaySFXOneShot("SFX_Citizen_Inbed_F_Full", GameMgr.Instance._CamMgr.m_MainCam.transform.position, base.transform.position);
                                    if (__instance.m_AccessoryInfo != null && __instance.m_AccessoryInfo.Name == "Ratdrake" && t_Citizen.GetMaxHP() > t_Citizen.m_CurHP)
                                    {
                                        t_Citizen.Heal(t_Citizen.GetMaxHP() - t_Citizen.m_CurHP, _effect: true);
                                        t_Citizen.m_UnitHP.UpdateHp(t_Citizen.m_CurHP / t_Citizen.GetMaxHP());
                                    }
                                }
                            }
                            else
                            {
                                GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Someone use"), Color.red);
                            }

                            return;
                        }

                        if (__instance.m_CheckBox.m_Building.m_Info.Ability == BuildAbility.House)
                        {
                            if (__instance.m_CheckBox.m_Building.m_BuildInfoUI.IsGuestReady())
                            {
                                if (!__instance.IsInjuryCitizenGather())
                                {
                                    return;
                                }

                                int gatherUnitIndex3 = __instance.m_GatherUnitIndex;
                                __instance.DropAction();
                                T_Citizen t_Citizen2 = GameMgr.Instance._T_UnitMgr.FindCitizen(gatherUnitIndex3);
                                if (t_Citizen2 != null)
                                {
                                    (__instance.m_CheckBox.m_Building as Building_House).GuestSet(t_Citizen2);
                                    AudioController.PlaySFXOneShot("SFX_Citizen_Inbed_F_Full", GameMgr.Instance._CamMgr.m_MainCam.transform.position, base.transform.position);
                                    if (__instance.m_AccessoryInfo != null && __instance.m_AccessoryInfo.Name == "Ratdrake" && t_Citizen2.GetMaxHP() > t_Citizen2.m_CurHP)
                                    {
                                        t_Citizen2.Heal(t_Citizen2.GetMaxHP() - t_Citizen2.m_CurHP, _effect: true);
                                        t_Citizen2.m_UnitHP.UpdateHp(t_Citizen2.m_CurHP / t_Citizen2.GetMaxHP());
                                    }
                                }
                            }
                            else
                            {
                                GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Someone use"), Color.red);
                            }

                            return;
                        }

                        if (__instance.m_CheckBox.m_Building.m_Info.Ability == BuildAbility.GraveStone || __instance.m_CheckBox.m_Building.m_Info.Ability == BuildAbility.Gravegarden)
                        {
                            if (__instance.m_CheckBox.m_Building.IsFunctionOK())
                            {
                                int gatherUnitIndex4 = __instance.m_GatherUnitIndex;
                                if (GameMgr.Instance._T_UnitMgr.FindDeathCitizen(gatherUnitIndex4) != null)
                                {
                                    if (DefinesGetValue<bool>("IsMouseTargetOn"))
                                    {
                                        __instance.FlipX(__instance.m_CheckBox.m_Building.Tf.position.x > __instance.Tf.position.x);
                                    }

                                     __instance.DropAction();
                                    (__instance.m_CheckBox.m_Building as Building_GraveStone).Add_DeathCitizen(gatherUnitIndex4);
                                    __instance.m_CheckBox.m_Building.Building_Update3();
                                }
                            }
                            else
                            {
                                GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/No more storage"), Color.red);
                            }

                            return;
                        }

                        if (__instance.m_CheckBox.m_Building.m_Info.Name == BuildingName.Prison)
                        {
                            if (__instance.m_CheckBox.m_Building.IsFunction2OK())
                            {
                                int gatherUnitIndex5 = __instance.m_GatherUnitIndex;
                                Building_Prison building_Prison = __instance.m_CheckBox.m_Building as Building_Prison;
                                T_Citizen citizen2 = GameMgr.Instance._T_UnitMgr.FindCitizen(gatherUnitIndex5);
                                if (building_Prison.CanI_Imprison(citizen2))
                                {
                                    __instance.DropAction();
                                    building_Prison.Add_Citizen(gatherUnitIndex5);
                                    AudioController.PlaySFXOneShot("SFX_Citizen_InPrison_F_Full", GameMgr.Instance._CamMgr.m_MainCam.transform.position, base.transform.position);
                                }
                            }
                            else
                            {
                                GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/No more storage"), Color.red);
                            }

                            return;
                        }
                    }
                }

                if (InputMgr.GetKeyDown(HotKeyName.DetailCheck))
                {
                    float fadeTime = 0.3f;
                    if ((__instance.m_AniState == AniState.Idle || __instance.m_AniState == AniState.Walking || __instance.m_AniState == AniState.Dash) && (__instance.m_CharState == CharState.None || __instance.m_CharState == CharState.Carrying) && !GameMgr.Instance._SysMgr.IsGamePause())
                    {
                        if (__instance.m_CheckBox.AreYouSelectCtizien())
                        {
                            __instance.CharacterStop();
                            GameMgr.Instance._SysMgr.SystemPause(pause: true);
                            T_Citizen citizen = __instance.m_CheckBox.GetSelectCitizen();
                            float nowCameraScale = GameMgr.Instance._CamMgr.m_MainCam.orthographicSize;
                            GameMgr.Instance._CamMgr.CameraZoomSmooth(GameMgr.Instance._CamMgr.m_DefaultCameraSize, fadeTime, limit_check: true);
                            GameMgr.Instance._CamMgr.MoveByIgnoreDeadzone(citizen.Tf.position, fadeTime, delegate
                            {
                                GameMgr.Instance._CitizenInfoUI.SetOnClosedListener(delegate
                                {
                                    InputMgr.Instance.SetDefaultActionMap();
                                    GameMgr.Instance._CamMgr.CameraZoomSmooth(nowCameraScale, fadeTime, limit_check: true);
                                    GameMgr.Instance._CamMgr.MoveByIgnoreDeadzone(__instance.Tf.position + new Vector3(0f, __instance.m_CamBottomHeight, 0f), fadeTime);
                                    GameMgr.Instance._CitizenInfoUI.SetOnClosedListener(null);
                                });
                                GameMgr.Instance._CitizenInfoUI.Show(citizen, isPause: true);
                            });
                            return;
                        }

                        if (__instance.m_CheckBox.IsBuildInfoOn() && !GameMgr.Instance._BatchUI.m_BatchFrameCoolTime)
                        {
                            __instance.CharacterStop();
                            Building building2 = __instance.m_CheckBox.m_Building;
                            if (building2 != null && building2.m_Info.Name != BuildingName.EnemyNexus)
                            {
                                GameMgr.Instance._BuildMidUI.BuildMid_Open(building2.m_BuildInfoUI, delegate
                                {
                                    InputMgr.Instance.SetDefaultActionMap();
                                });
                            }

                            return;
                        }

                        if (__instance.m_CheckBox.AreYouSelectMapObjType(MapObjName.BattlefieldMerchant))
                        {
                            __instance.CharacterStop();
                            GameMgr.Instance._SysMgr.SystemPause(pause: true);
                            InputMgr.Instance.SetActionMap(InputMgr.INPUT_ACTIONMAP_UI);
                            GameMgr.Instance._ConstructUI.OpenTab(5);
                            return;
                        }
                    }
                }
                else if (InputMgr.GetKeyUp(HotKeyName.MoveNext))
                {
                    if (__instance.m_CheckBox.IsInfoBoxOn())
                    {
                        __instance.m_CheckBox.BoxSelectUpdate();
                        __instance.BehaviorStop();
                        return;
                    }
                }
                //钓鱼？
                else if (InputMgr.GetKeyDown(HotKeyName.DropObj) && __instance.m_CheckBox.AreYouContainType(MiniType.Tile) && __instance.m_CheckBox.m_Tile != null && __instance.m_CheckBox.m_Tile.m_TileType == TileType.Water && __instance.m_CheckBox.Pos_Center.y == -1 && __instance.m_CheckBox.Pos_Center.x != 0 && __instance.IsSpaceOut == 0 && __instance.IsExistQueenAbility(Res_Ability.OQ_Fishing) && __instance.m_CharState != CharState.Carrying && __instance.m_CheckBox.AreYouSelectType(MiniType.Tile) && !__instance.IsAirWalk() && !__instance.IsFalling())
                {
                    if (GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x + __instance.m_CheckBox.Pos_Center.x, __instance.m_CurNode.y) != null && GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x + __instance.m_CheckBox.Pos_Center.x, __instance.m_CurNode.y).m_NodeType != NodeType.Wall)
                    {
                        GameMgr.Instance._T_UnitMgr.m_Queen.DoFishing();
                        return;
                    }
                }
                //其他（主要）
                else
                {
                    if (InputMgr.GetKeyDown(HotKeyName.DropObj) && __instance.m_CheckBox.IsInfoBoxOn() && __instance.IsSpaceOut == 0 && __instance.IsExistQueenAbility(Res_Ability.OQ_StealDeathBody) && __instance.m_CheckBox.AreYouSelectUnit(UnitKind.Citizen) && /*__instance.m_CheckBox.GetSelectCitizen().__instance.m_CharState == CharState.Death &&*/ __instance.m_CheckBox.GetSelectCitizen().m_Gold > 0f && __instance.m_CharState != CharState.Carrying)
                    {
                        if (!__instance.m_QueenInteract)
                        {
                            GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Cant ado battlefield"), Color.red);
                            return;
                        }

                        __instance.m_TargetUnit = __instance.m_CheckBox.GetSelectCitizen();
                        if (DefinesGetValue<bool>("IsMouseTargetOn"))
                        {
                            if (__instance.m_TargetUnit.Tf.position.x > __instance.Tf.position.x)
                            {
                               __instance.FlipX(right: true);
                            }
                            else
                            {
                               __instance.FlipX(right: false);
                            }
                        }

                        T_Citizen t_Citizen3 = __instance.m_TargetUnit as T_Citizen;
                        __instance.SetCharState(CharState.Gathering);
                        __instance.Kill_MoveCoroutine(GameUnit.Kill_MoveState.Only_corMoving);
                        if (t_Citizen3 != null && t_Citizen3.m_CharState == CharState.Death)
                        {
                            float gold = t_Citizen3.m_Gold;
                            GameMgr.Instance._EcoMgr.Country_GetGold_Refund("Pickpocket", t_Citizen3.m_Gold);
                            t_Citizen3.m_Gold = 0f;
                            __instance.PlayAniOneShot_EndIdle(AniState.Idle, "Priscess_Thievery");
                            __instance.m_CheckBox.HotKeyUpdate();
                            AudioController.PlaySFXOneShot("SFX_Effect_GoldGet", GameMgr.Instance._CamMgr.m_MainCam.transform.position, __instance.Tf.position);
                            GameMgr.Instance._PoolMgr.Pool_GetEffect.GetNextObj().GetComponent<GetEffect>().GetKindEffect(GetKind.Gold, __instance, gold, new Vector3(0f, 0.5f, 0f));
                        }

                        return;
                    }

                    if (InputMgr.GetKeyDown(HotKeyName.DropObj) && __instance.m_CheckBox.IsInfoBoxOn() && __instance.IsSpaceOut == 0 && __instance.List_QueenAbility.Exists((QueenAbilityInfo x) => x.List_Ability.Contains(Res_Ability.OQ_CureDisease)) && __instance.m_CheckBox.AreYouSelectUnit(UnitKind.Citizen) && __instance.m_CheckBox.GetSelectCitizen().IsDisease() && !__instance.m_CheckBox.GetSelectCitizen().m_ImprisonCheck && /*__instance.m_CheckBox.GetSelectCitizen().__instance.m_CharState != CharState.Death &&*/ __instance.m_CharState != CharState.Carrying && !__instance.m_WorkTimeGauge.IsGaugeAcitve())
                    {
                        if (!__instance.m_QueenInteract)
                        {
                            GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Cant do battlefield"), Color.red);
                            return;
                        }

                        if (__instance.m_CheckBox.GetSelectCitizen().m_ImFatigue == 4)
                        {
                            GameMgr.Instance._CenterAlarmUI.CenterAlarmSet(C_AlarmState.Cant_Batch_InjuryCitizen);
                            return;
                        }

                        __instance.m_TargetUnit = __instance.m_CheckBox.GetSelectCitizen();
                        if (DefinesGetValue<bool>("IsMouseTargetOn"))
                        {
                            if (__instance.m_TargetUnit.Tf.position.x > __instance.Tf.position.x)
                            {
                               __instance.FlipX(right: true);
                            }
                            else
                            {
                               __instance.FlipX(right: false);
                            }
                        }

                        T_Citizen target_unit3 = __instance.m_TargetUnit as T_Citizen;
                        __instance.SetCharState(CharState.Gathering);
                        __instance.Kill_MoveCoroutine(GameUnit.Kill_MoveState.Only_corMoving);
                        target_unit3.m_Unit = __instance;
                        if (target_unit3.m_UseBuildingStep == UseBuildingStep.UsingBuilding)
                        {
                            target_unit3.UseBuildingKill(4);
                        }

                        target_unit3.BehaviorStop();
                        target_unit3.KillMoving(_fall_check: false);
                        target_unit3.Drop_GatheringList();
                        target_unit3.CitizenChaosSet(5f);
                        target_unit3.Call_SpecialAni(AniState.Tax_Collection, "Perk_Recovery2", 4.46f, _loop: false, !__instance.IsFlipX(), Idle_Back: false, _killcor: true);
                        EffectActive effect = delegate
                        {
                            __instance.m_WorkTimeGauge.StopGauge();
                            if (target_unit3.Obj.activeSelf)
                            {
                                target_unit3.KillDisease();
                            }
                        };
                        __instance.m_WorkTimeGauge.GaugeActive_RT(__instance.m_TargetUnit, effect, 4.46f);
                        __instance.SetAniState(AniState.Gathering, "Perk_Recovery", _loop: false, _now: false);
                        return;
                    }

                    if (InputMgr.GetKeyDown(HotKeyName.DropObj) && __instance.m_CheckBox.IsInfoBoxOn() && __instance.m_WorldObj != null && __instance.List_QueenAbility.Exists((QueenAbilityInfo x) => x.List_Ability.Contains(Res_Ability.OQ_SprayWater)) && __instance.m_CheckBox.AreYouSelectType(MiniType.WorldObj) && !__instance.m_WorldObj.m_Gatherd && __instance.m_CharState != CharState.Carrying)
                    {
                        if (!__instance.m_WorldObj.IsExistBuff(DefinesGetValue<string>("Str_PBuff_PlantCare")))
                        {
                            if (!__instance.m_QueenInteract)
                            {
                                GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Cant do battlefield"), Color.red);
                                return;
                            }

                            if (!__instance.m_CheckBox.PlantTypeCheck())
                            {
                                GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Cant gather burry plant"), Color.red);
                                return;
                            }

                            if (__instance.m_WorldObj.m_Unit != null && __instance.m_WorldObj.m_Unit.m_WorkTimeGauge.IsGaugeAcitve())
                            {
                                __instance.m_WorldObj.m_Unit.BehaviorStop();
                            }

                            if (DefinesGetValue<bool>("IsMouseTargetOn"))
                            {
                               __instance.FlipX(__instance.m_WorldObj.Tf.position.x > __instance.Tf.position.x);
                            }

                            __instance.SetCharState(CharState.Gathering);
                            __instance.Kill_MoveCoroutine(GameUnit.Kill_MoveState.Only_corMoving);
                            __instance.m_WorldObj.m_Unit = __instance;
                            __instance.PlayAniOneShot_EndIdle(AniState.Idle, "Gardening");
                            return;
                        }

                        GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Already has growth buff"), Color.red);
                    }
                    //主要交互：不在移动时
                    else if (!InputMgr.GetKeyUp_F(HotKeyName.Interaction) && InputMgr.GetKey(HotKeyName.Interaction) && __instance.m_MoveDir != MoveDir.Right_Down && __instance.m_MoveDir != MoveDir.Left_Down && __instance.m_MoveDir != MoveDir.Left_Drop && __instance.m_MoveDir != MoveDir.Right_Drop)
                    {
                        //植物蓝图
                        if (__instance.m_CheckBox.IsBP_PlantConstructEnable())
                        {
                            if (InputMgr.GetKeyDown(HotKeyName.Interaction) && (__instance.m_CharState == CharState.None || __instance.m_CharState == CharState.Carrying) && __instance.m_AniState == AniState.Idle)
                            {
                                //if (DefinesGetValue<bool>("IsMouseTargetOn"))
                                //{
                                //    __instance.FlipX(__instance.m_CheckBox.m_BP_Plant.__instance.Tf.position.x > __instance.Tf.position.x);
                                //}

                                if (__instance.m_CharState == CharState.Carrying || __instance.List_Gathering.Count != 0)
                                {
                                    __instance.DropAction();
                                }

                                if (__instance.m_CheckBox.m_BP_Plant.m_Unit != null)
                                {
                                    __instance.m_CheckBox.m_BP_Plant.m_Unit = null;
                                }

                                __instance.SetCharState(CharState.Planting);
                                __instance.m_CheckBox.m_BP_Plant.m_Unit = __instance;
                                __instance.SetAniState(AniState.Harvesting, _loop: true, _now: false);
                                return;
                            }
                        }
                        //建筑蓝图
                        else if (__instance.m_CheckBox.IsBP_ConstructEnable())
                        {
                            if (InputMgr.GetKeyDown(HotKeyName.Interaction) && (__instance.m_CharState == CharState.None || __instance.m_CharState == CharState.Carrying) && __instance.m_AniState == AniState.Idle)
                            {
                                //if (DefinesGetValue<bool>("IsMouseTargetOn"))
                                //{
                                //    __instance.FlipX(__instance.m_CheckBox.m_BP_Building.__instance.Tf.position.x > __instance.Tf.position.x);
                                //}

                                if (__instance.m_CharState == CharState.Carrying || __instance.List_Gathering.Count != 0)
                                {
                                    __instance.DropAction();
                                }

                                if (__instance.m_CheckBox.m_BP_Building.m_Unit != null)
                                {
                                    __instance.m_CheckBox.m_BP_Building.m_Unit = null;
                                }

                                __instance.SetCharState(CharState.Building);
                                __instance.m_CheckBox.m_BP_Building.m_Unit = __instance;
                                __instance.SetAniState(AniState.Building, _loop: true, _now: false);
                                return;
                            }
                        }
                        //其他
                        else
                        {
                            //蓝图启用，但未在携带中时（材料不足）
                            if (__instance.m_CheckBox.IsBP_FormEnable() && __instance.m_CharState != CharState.Carrying && InputMgr.GetKeyDown(HotKeyName.Interaction))
                            {
                                GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Need material to build"), Color.red);
                                return;
                            }

                            //修理
                            if (__instance.m_CheckBox.IsRepairEnable() > 0 && __instance.m_QueenInteract)
                            {
                                if (InputMgr.GetKeyDown(HotKeyName.Interaction) && __instance.m_CharState == CharState.None && __instance.m_AniState == AniState.Idle && __instance.List_Gathering.Count == 0 && __instance.m_CheckBox.IsRepairEnable() == 1)
                                {
                                    if (DefinesGetValue<bool>("IsMouseTargetOn") && __instance.m_CheckBox.m_Building != null)
                                    {
                                        __instance.FlipX(__instance.m_CheckBox.m_Building.Tf.position.x > __instance.Tf.position.x);
                                    }

                                    if (!GameMgr.Instance._SysMgr.List_RepairMark.Exists((WorkMark x) => x.m_Building == __instance.m_CheckBox.m_Building))
                                    {
                                        WorkMark component = GameMgr.Instance._PoolMgr.Pool_WorkMark.GetNextObj().GetComponent<WorkMark>();
                                        component.MarkSet((int)__instance.m_CheckBox.m_Building.Pos_Tile.x, (int)__instance.m_CheckBox.m_Building.Pos_Tile.y, _el_check: false, WorkMarkKind.Repair);
                                        component.MarkRefresh(__instance.m_CheckBox.m_Building);
                                    }

                                    if (__instance.m_CheckBox.m_Building.m_Unit != null && __instance.m_CheckBox.m_Building.m_Unit != __instance && __instance.m_CheckBox.m_Building.m_Unit.m_UnitKind == UnitKind.Citizen)
                                    {
                                        __instance.m_CheckBox.m_Building.m_Unit.GetComponent<T_Citizen>()?.GaugeVarCalculate(TypeOrder.Repairing);
                                        __instance.m_CheckBox.m_Building.m_Unit.GetComponent<T_Citizen>()?.StateReset();
                                    }

                                    __instance.m_CheckBox.m_Building.m_Unit = __instance;
                                    __instance.SetCharState(CharState.Building);
                                    __instance.m_CheckBox.m_Building.m_Gauge.gameObject.SetActive(value: true);
                                    __instance.SetAniState(AniState.Building, _loop: true, _now: false);
                                    return;
                                }

                                if (InputMgr.GetKeyDown(HotKeyName.Interaction) && __instance.m_CheckBox.IsRepairEnable() == 2)
                                {
                                    if (DefinesGetValue<bool>("IsMouseTargetOn") && __instance.m_CheckBox.m_Tile != null)
                                    {
                                        __instance.FlipX(__instance.m_CheckBox.m_Tile.Tf.position.x > __instance.Tf.position.x);
                                    }

                                    if (!GameMgr.Instance._SysMgr.List_RepairMark.Exists((WorkMark x) => x.m_Tile == __instance.m_CheckBox.m_Tile) && !__instance.m_CheckBox.m_Tile.IsNatureLadder)
                                    {
                                        WorkMark component2 = GameMgr.Instance._PoolMgr.Pool_WorkMark.GetNextObj().GetComponent<WorkMark>();
                                        component2.m_Tile = __instance.m_CheckBox.m_Tile;
                                        component2.MarkSet(__instance.m_CheckBox.m_Tile.m_X, __instance.m_CheckBox.m_Tile.m_Y, _el_check: false, WorkMarkKind.Repair);
                                        component2.m_Tile.MakeEnableList();
                                    }

                                    if (__instance.m_CheckBox.m_Tile.IsBuilding && !__instance.m_CheckBox.m_Tile.IsNatureLadder)
                                    {
                                        if (__instance.m_CheckBox.m_Tile.m_Unit != null && __instance.m_CheckBox.m_Tile.m_Unit.m_WorkTimeGauge.IsGaugeAcitve())
                                        {
                                            __instance.m_CheckBox.m_Tile.m_Unit.BehaviorStop();
                                        }

                                        __instance.SetCharState(CharState.Building);
                                        __instance.Kill_MoveCoroutine(GameUnit.Kill_MoveState.Only_corMoving);
                                        __instance.m_CheckBox.m_Tile.m_Unit = __instance;
                                        //__instance.m_WorkTimeGauge.GaugeActive(__instance.m_CheckBox.m_Tile, Effect);
                                        __instance.SetAniState(AniState.Building, _loop: true, _now: false);
                                        return;
                                    }
                                }
                            }
                            //拆除
                            else if (__instance.m_CheckBox.IsDemolitionEnable() > 0)
                            {
                                if (InputMgr.GetKeyDown(HotKeyName.Interaction) && __instance.m_CharState == CharState.None && __instance.m_AniState == AniState.Idle && __instance.List_Gathering.Count == 0 && __instance.m_CheckBox.IsDemolitionEnable() == 1)
                                {
                                    if (__instance.m_CheckBox.m_Building.m_Unit != null && __instance.m_CheckBox.m_Building.m_Unit != __instance && __instance.m_CheckBox.m_Building.m_Unit.m_UnitKind == UnitKind.Citizen)
                                    {
                                        __instance.m_CheckBox.m_Building.m_Unit.GetComponent<T_Citizen>().GaugeVarCalculate(TypeOrder.Demolition);
                                        __instance.m_CheckBox.m_Building.m_Unit.GetComponent<T_Citizen>().StateReset();
                                    }

                                    if (DefinesGetValue<bool>("IsMouseTargetOn"))
                                    {
                                        __instance.OnlyFlipX(__instance.m_CheckBox.m_Building.Tf.position.x > __instance.Tf.position.x);
                                    }

                                    __instance.m_CheckBox.m_Building.m_Unit = __instance;
                                    __instance.SetCharState(CharState.Building);
                                    __instance.m_CheckBox.m_Building.m_Gauge.gameObject.SetActive(value: true);
                                    __instance.SetAniState(AniState.Building, _loop: true, _now: false);
                                    return;
                                }

                                if (InputMgr.GetKeyDown(HotKeyName.Interaction) && __instance.m_CheckBox.IsInfoBoxOn() && __instance.m_CheckBox.IsDemolitionEnable() == 2 && __instance.m_CheckBox.m_Tile.IsBuilding && GameMgr.Instance._SysMgr.List_DemolitionMark.Exists((WorkMark x) => x.m_Tile == __instance.m_CheckBox.m_Tile))
                                {
                                    if (__instance.m_CheckBox.m_Tile.m_Unit != null && __instance.m_CheckBox.m_Tile.m_Unit != __instance && __instance.m_CheckBox.m_Tile.m_Unit.m_UnitKind == UnitKind.Citizen)
                                    {
                                        __instance.m_CheckBox.m_Tile.m_Unit.GetComponent<T_Citizen>().GaugeVarCalculate(TypeOrder.Demolition);
                                        __instance.m_CheckBox.m_Tile.m_Unit.GetComponent<T_Citizen>().StateReset();
                                    }

                                    __instance.SetCharState(CharState.Building);
                                    if (DefinesGetValue<bool>("IsMouseTargetOn"))
                                    {
                                        __instance.OnlyFlipX(__instance.m_CheckBox.m_Tile.Tf.position.x > __instance.Tf.position.x);
                                    }

                                    __instance.Kill_MoveCoroutine(GameUnit.Kill_MoveState.Only_corMoving);
                                    __instance.m_CheckBox.m_Tile.m_Unit = __instance;
                                    //__instance.m_WorkTimeGauge.GaugeActive(__instance.m_CheckBox.m_Tile, Effect);
                                    __instance.SetAniState(AniState.Building, _loop: true, _now: false);
                                    return;
                                }
                            }
                            //携带物品中
                            else if (__instance.m_CharState == CharState.Carrying && __instance.List_Gathering.Count != 0)
                            {
                                if (__instance.m_CheckBox.IsInfoBoxOn() && __instance.m_WorldObj != null && __instance.m_CheckBox.AreYouSelectType(MiniType.WorldObj))
                                {
                                    if (InputMgr.GetKeyDown(HotKeyName.Interaction))
                                    {
                                        GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Carrying state alarm"), Color.red);
                                        GameMgr.Instance._NpcAlarmUI.DropObjTutoCheck();
                                    }
                                }
                                else if (__instance.m_CheckBox.IsInfoBoxOn() && __instance.m_CheckBox.m_Tile != null && __instance.m_CheckBox.AreYouSelectType(MiniType.Tile) && !__instance.m_CheckBox.m_Tile.IsBuilding && InputMgr.GetKeyDown(HotKeyName.Interaction))
                                {
                                    GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Carrying state alarm"), Color.red);
                                    GameMgr.Instance._NpcAlarmUI.DropObjTutoCheck();
                                }
                            }
                            //闲置待机状态
                            else if (__instance.m_CharState == CharState.None && __instance.m_AniState == AniState.Idle)
                            {
                                //存在交互对象（tile）
                                if ((DefinesGetValue<bool>("IsMouseTargetOn") ? InputMgr.GetKey(HotKeyName.Interaction) : InputMgr.GetKey(HotKeyName.Interaction)) && __instance.m_CheckBox.IsInfoBoxOn() && __instance.m_CheckBox.m_Tile != null && __instance.m_CheckBox.AreYouSelectType(MiniType.Tile))
                                {
                                    if (!__instance.m_QueenInteract)
                                    {
                                        if (InputMgr.GetKeyDown(HotKeyName.Interaction))
                                        {
                                            GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Cant do battlefield"), Color.red);
                                        }

                                        return;
                                    }

                                    //非建筑或自然生成的梯子（挖掘对象）
                                    if (!__instance.m_CheckBox.m_Tile.IsBuilding || (__instance.m_CheckBox.m_Tile.IsBuilding && __instance.m_CheckBox.m_Tile.IsNatureLadder))
                                    {
                                        if (__instance.m_CheckBox.m_Tile.m_Unit != null && __instance.m_CheckBox.m_Tile.m_Unit.m_WorkTimeGauge.IsGaugeAcitve())
                                        {
                                            if (__instance.m_CheckBox.m_Tile.m_Unit.IsMoveState())
                                            {
                                                __instance.m_CheckBox.m_Tile.m_Unit.KillMoving(_fall_check: false);
                                            }

                                            __instance.m_CheckBox.m_Tile.m_Unit.BehaviorStop();
                                        }

                                        if (DefinesGetValue<bool>("IsMouseTargetOn"))
                                        {
                                            __instance.FlipX(__instance.m_CheckBox.m_Tile.Tf.position.x > __instance.Tf.position.x);
                                        }

                                        __instance.SetCharState(CharState.Mining);
                                        EffectActive effect2 = delegate
                                        {
                                            if (__instance.m_CheckBox.m_Tile != null)
                                            {
                                                if (__instance.m_CheckBox.m_Tile.m_TileType == TileType.Water)
                                                {
                                                    __instance.m_CheckBox.m_Tile.WaterMining(__instance, -1, first: true);
                                                }
                                                else
                                                {
                                                    GameMgr.Instance._SysMgr.MiningUpdate(__instance.m_CheckBox.m_Tile.Tf.position);
                                                    __instance.m_CheckBox.m_Tile.DestroyTile(Dir_check: true);
                                                }
                                            }

                                            __instance.SetCharState(CharState.None);
                                            __instance.m_WorkTimeGauge.StopGauge(TypeOrder.Mining);
                                            __instance.SetAniState(AniState.Idle, _loop: true, _now: false);
                                        };
                                        __instance.Kill_MoveCoroutine(GameUnit.Kill_MoveState.Only_corMoving);
                                        __instance.m_CheckBox.m_Tile.m_Unit = __instance;
                                        __instance.m_WorkTimeGauge.GaugeActive(__instance.m_CheckBox.m_Tile, effect2);
                                        __instance.SetAniState(AniState.Mining, _loop: true, _now: false);
                                        return;
                                    }
                                }
                                //没有tile对象时
                                else
                                {
                                    //选中世界物品，采集资源
                                    if ((DefinesGetValue<bool>("IsMouseTargetOn") ? InputMgr.GetKey(HotKeyName.Interaction) : InputMgr.GetKeyDown(HotKeyName.Interaction)) && __instance.m_CheckBox.IsInfoBoxOn() && __instance.m_WorldObj != null && __instance.m_CheckBox.AreYouSelectType(MiniType.WorldObj) && !__instance.m_WorldObj.m_Gatherd)
                                    {
                                        if (!__instance.m_QueenInteract)
                                        {
                                            if (InputMgr.GetKeyDown(HotKeyName.Interaction))
                                            {
                                                GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Cant do battlefield"), Color.red);
                                            }

                                            return;
                                        }

                                        if (!__instance.m_CheckBox.PlantTypeCheck())
                                        {
                                            if (InputMgr.GetKeyDown(HotKeyName.Interaction))
                                            {
                                                GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Cant gather burry plant"), Color.red);
                                            }

                                            return;
                                        }

                                        if (__instance.m_WorldObj.m_Unit != null && __instance.m_WorldObj.m_Unit.m_WorkTimeGauge.IsGaugeAcitve())
                                        {
                                            __instance.m_WorldObj.m_Unit.BehaviorStop();
                                        }

                                        if (DefinesGetValue<bool>("IsMouseTargetOn"))
                                        {
                                            __instance.FlipX(__instance.m_WorldObj.Tf.position.x > __instance.Tf.position.x);
                                        }

                                        __instance.SetCharState(CharState.Gathering);
                                        __instance.Kill_MoveCoroutine(GameUnit.Kill_MoveState.Only_corMoving);
                                        __instance.m_WorldObj.m_Unit = __instance;
                                        EffectActive effect3 = delegate
                                        {
                                            if (__instance.m_WorldObj != null)
                                            {
                                                EncyclopediaManager.SetUnlock($"{__instance.m_WorldObj.m_Info.NameType}");
                                            }

                                            __instance.m_WorkTimeGauge.StopGauge(TypeOrder.Gathering);
                                            if (__instance.m_WorldObj != null && !__instance.m_WorldObj.m_Gatherd)
                                            {
                                                __instance.m_WorldObj.Interaction(__instance);
                                            }

                                            __instance.SetAniState(AniState.Idle, _loop: true, _now: false);
                                            GameMgr.Instance._T_UnitMgr.m_Queen.m_CheckBox.HotKeyUpdate();
                                        };
                                        __instance.m_WorkTimeGauge.GaugeActive(__instance.m_WorldObj, effect3);
                                        if (__instance.m_WorldObj.m_Info.NameType == TileType.SeaanemonePlant)
                                        {
                                            EncyclopediaManager.SetUnlock($"{__instance.m_WorldObj.m_Info.NameType}");
                                        }

                                        if (__instance.m_WorldObj.m_Info.Category == PlantCategory.Tree || __instance.m_WorldObj.m_Info.Category == PlantCategory.Install)
                                        {
                                            __instance.SetAniState(AniState.Felling, _loop: true, _now: false);
                                        }
                                        else
                                        {
                                            __instance.SetAniState(AniState.Harvesting, _loop: true, _now: false);
                                        }

                                        return;
                                    }

                                    //选中圣甲虫
                                    if ((DefinesGetValue<bool>("IsMouseTargetOn") ? InputMgr.GetKey(HotKeyName.Interaction) : InputMgr.GetKeyDown(HotKeyName.Interaction)) && __instance.m_CheckBox.IsInfoBoxOn() &&  __instance.m_Animal != null && __instance.m_CheckBox.AreYouSelectType(MiniType.Animal) &&  __instance.m_Animal.m_Info.m_Name == AnimalName.PharaohScarab)
                                    {
                                         __instance.m_Animal.SetDeath(_mat: false, Unit_Attacekd_Tag.None);
                                        return;
                                    }

                                    //受伤或死亡的市民
                                    if ((DefinesGetValue<bool>("IsMouseTargetOn") ? InputMgr.GetKey(HotKeyName.Interaction) : InputMgr.GetKeyDown(HotKeyName.Interaction)) && __instance.m_CheckBox.IsInfoBoxOn() && __instance.m_CheckBox.AreYouSelectInjuryUnit() && (__instance.m_CheckBox.GetSelectCitizen().m_CharState == CharState.Injury || __instance.m_CheckBox.GetSelectCitizen().m_CharState == CharState.Death))
                                    {
                                        if (!__instance.m_QueenInteract)
                                        {
                                            if (InputMgr.GetKeyDown(HotKeyName.Interaction))
                                            {
                                                GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Cant do battlefield"), Color.red);
                                            }

                                            return;
                                        }

                                        __instance.m_TargetUnit = __instance.m_CheckBox.GetSelectCitizen();
                                        if ( __instance.m_TargetUnit.IsAlliance)
                                        {
                                            return;
                                        }

                                        if (DefinesGetValue<bool>("IsMouseTargetOn"))
                                        {
                                            __instance.FlipX( __instance.m_TargetUnit.Tf.position.x > __instance.Tf.position.x);
                                        }

                                        GameUnit target_unit2 = __instance.m_TargetUnit;
                                        __instance.SetCharState(CharState.Gathering);
                                        __instance.Kill_MoveCoroutine(GameUnit.Kill_MoveState.Only_corMoving);
                                        target_unit2.m_Unit = __instance;
                                        __instance.m_GatherUnitIndex = target_unit2.m_ID;
                                        EffectActive effect4 = delegate
                                        {
                                            __instance.m_WorkTimeGauge.StopGauge();
                                            if (target_unit2.Obj.activeSelf)
                                            {
                                                TileObject component4 = GameMgr.Instance._PoolMgr.Pool_TileObject.GetNextObj(_act: false).GetComponent<TileObject>();
                                                if (target_unit2.m_UnitKind == UnitKind.GBot)
                                                {
                                                    component4.ObjectInit(TileType.GBot, TObjState.Basic, new Vector3(0f, 0f, target_unit2.m_ID));
                                                }
                                                else
                                                {
                                                    component4.ObjectInit(TileType.A_Citizen, TObjState.Basic, new Vector3(0f, 0f, target_unit2.m_ID));
                                                }

                                                component4.ObjectGathered(__instance, pick_ani: true);
                                                __instance.SetCharState(CharState.Carrying);
                                                __instance.SetAniState(AniState.Gathering, _loop: false, _now: false);
                                                target_unit2.gameObject.SetActive(value: false);
                                            }
                                        };
                                        __instance.m_WorkTimeGauge.GaugeActive( __instance.m_TargetUnit, effect4);
                                        __instance.SetAniState(AniState.Gathering, _loop: true, _now: false);
                                        return;
                                    }

                                    //罪犯
                                    if ((DefinesGetValue<bool>("IsMouseTargetOn") ? InputMgr.GetKey(HotKeyName.Interaction) : InputMgr.GetKeyDown(HotKeyName.Interaction)) && __instance.m_CheckBox.IsInfoBoxOn() && __instance.IsSpaceOut == 0 && __instance.m_CheckBox.AreYouSelectUnit(UnitKind.Citizen) && __instance.m_CheckBox.GetSelectCitizen().m_Buff.IsCriminal() && !__instance.m_CheckBox.GetSelectCitizen().m_ImprisonCheck)
                                    {
                                        if (!__instance.m_QueenInteract)
                                        {
                                            if (InputMgr.GetKeyDown(HotKeyName.Interaction))
                                            {
                                                GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Cant do battlefield"), Color.red);
                                            }

                                            return;
                                        }

                                        if (__instance.m_CheckBox.GetSelectCitizen().m_ImFatigue == 4)
                                        {
                                            if (InputMgr.GetKeyDown(HotKeyName.Interaction))
                                            {
                                                GameMgr.Instance._CenterAlarmUI.CenterAlarmSet(C_AlarmState.Cant_Batch_InjuryCitizen);
                                            }

                                            return;
                                        }

                                        __instance.m_TargetUnit = __instance.m_CheckBox.GetSelectCitizen();
                                        if (DefinesGetValue<bool>("IsMouseTargetOn"))
                                        {
                                            if ( __instance.m_TargetUnit.Tf.position.x > __instance.Tf.position.x)
                                            {
                                                __instance.FlipX(right: true);
                                            }
                                            else
                                            {
                                                __instance.FlipX(right: false);
                                            }
                                        }

                                        T_Citizen target_unit = __instance.m_TargetUnit as T_Citizen;
                                        __instance.SetCharState(CharState.Gathering);
                                        __instance.Kill_MoveCoroutine(GameUnit.Kill_MoveState.Only_corMoving);
                                        target_unit.m_Unit = __instance;
                                        if (target_unit.m_UseBuildingStep == UseBuildingStep.UsingBuilding)
                                        {
                                            target_unit.UseBuildingKill(4);
                                        }

                                        target_unit.BehaviorStop();
                                        target_unit.KillMoving(_fall_check: false);
                                        target_unit.Drop_GatheringList();
                                        if (__instance.List_QueenAbility.Exists((QueenAbilityInfo x) => x.List_Ability.Contains(Res_Ability.OQ_Reformation)))
                                        {
                                            target_unit.CitizenChaosSet(5f);
                                            target_unit.Call_SpecialAni(AniState.Tax_Collection, "Perk_lead2", 4.46f, _loop: true, !__instance.IsFlipX(), Idle_Back: true, _killcor: true);
                                            EffectActive effect5 = delegate
                                            {
                                                __instance.m_WorkTimeGauge.StopGauge();
                                                if (target_unit.Obj.activeSelf)
                                                {
                                                    target_unit.m_EventAction = 0;
                                                    target_unit.m_Buff.BuffKill(C_Buff.Reb_X);
                                                    //target_unit.m_Buff.RefKill(Defines.Str_DevilTemp);
                                                    target_unit.m_Buff.BuffKill(C_Buff.Criminal);
                                                    target_unit.m_Buff.BuffKill(C_Buff.Criminal_Lv2);
                                                    if (target_unit.m_CriminalEffect != null)
                                                    {
                                                        target_unit.m_CriminalEffect.DestroyEffect();
                                                    }

                                                    target_unit.SetAniState(AniState.Idle, _loop: true, _now: true);
                                                    GameMgr.Instance._T_UnitMgr.m_Queen.m_CheckBox.SelectBoxInfoUpdate();
                                                    if (GameMgr.Instance._FilterSelectUI.m_ActIndex == 4)
                                                    {
                                                        target_unit.FilterSet(0);
                                                        target_unit.FilterSet(4);
                                                    }
                                                }
                                            };
                                            __instance.m_WorkTimeGauge.GaugeActive_RT( __instance.m_TargetUnit, effect5, 4.46f);
                                            __instance.SetAniState(AniState.Gathering, "Perk_lead", _loop: false, _now: false);
                                            return;
                                        }

                                        target_unit.CitizenChaosSet(3f);
                                        target_unit.Call_SpecialAni(AniState.Tax_Collection, (UnityEngine.Random.Range(0, 2) == 0) ? "Arrested" : "Arrested_2", 3f, _loop: false, __instance.IsFlipX(), Idle_Back: false, _killcor: true);
                                        EffectActive effect6 = delegate
                                        {
                                            __instance.m_WorkTimeGauge.StopGauge();
                                            if (target_unit.Obj.activeSelf)
                                            {
                                                GameMgr.Instance._PoolMgr.Pool_TileObject.GetNextObj(_act: false).GetComponent<TileObject>().ObjectInit(TileType.A_Citizen, TObjState.Basic, new Vector3(0f, 0f, target_unit.m_ID))
                                                    .ObjectGathered(__instance, pick_ani: true);
                                                __instance.SetCharState(CharState.Carrying);
                                                __instance.SetAniState(AniState.Idle, _loop: true, _now: false);
                                                target_unit.gameObject.SetActive(value: false);
                                                target_unit.m_ArrestCheck = true;
                                            }
                                        };
                                        __instance.m_WorkTimeGauge.GaugeActive_RT( __instance.m_TargetUnit, effect6, 2.7f);
                                        __instance.SetAniState(AniState.Gathering, "Arreste", _loop: true, _now: false);
                                        return;
                                    }

                                    //市民
                                    if (InputMgr.GetKeyDown(HotKeyName.Interaction) && __instance.m_CheckBox.AreYouRealSelectType(MiniType.Citzien))
                                    {
                                        GameUnit selectGameUnit = __instance.m_CheckBox.GetSelectGameUnit();
                                        if ((object)selectGameUnit != null && selectGameUnit.m_E_Name == EnemyType.Shrew)
                                        {
                                            Shrew component3 = __instance.m_CheckBox.GetSelectGameUnit().GetComponent<Shrew>();
                                            if (__instance.m_CheckBox.GetSelectGameUnit().IsDisease())
                                            {
                                                component3.ShrewInteraction(_add: false);
                                            }
                                            else
                                            {
                                                component3.PlayKawaiAni();
                                            }

                                            __instance.m_CheckBox.HotKeyUpdate();
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //拾起地上的物品
                if ((__instance.m_AniState == AniState.Idle || __instance.m_AniState == AniState.Gathering) && (__instance.m_CharState == CharState.None || __instance.m_CharState == CharState.Carrying) && (DefinesGetValue<bool>("IsMouseTargetOn") ? InputMgr.GetKeyDown(HotKeyName.Interaction) : InputMgr.GetKeyDown(HotKeyName.Interaction)) && !InputMgr.GetKeyUp_F(HotKeyName.Interaction) && __instance.m_CheckBox.IsInfoBoxOn() && __instance.m_CheckBox.List_MiniInfo.Count > 0 && __instance.m_CheckBox.AreYouSelectType(MiniType.TileObj) && __instance.m_CheckBox.IsReadyToGatherTileObj())
                {
                    //物品未满
                    if (__instance.List_Gathering.Count < __instance.Get_HandCapacity())
                    {
                        if (DefinesGetValue<bool>("IsMouseTargetOn"))
                        {
                            if (__instance.m_CheckBox.GetSelectTObjPos_X(MiniType.TileObj) > __instance.Tf.position.x)
                            {
                                __instance.FlipX(right: true);
                            }
                            else
                            {
                                __instance.FlipX(right: false);
                            }
                        }

                        TileType selectTileObjType = __instance.m_CheckBox.GetSelectTileObjType();

                        //初次拾起时，播放拾取动画
                        if (__instance.List_Gathering.Count == 0)
                        {
                            __instance.SetAniState(AniState.Gathering, _loop: false, _now: false);
                        }

                        __instance.m_CheckBox.GatherSelected();
                        if (selectTileObjType != GameMgr.Instance._SysMgr.m_PileMatNumUI.m_Type)
                        {
                            __instance.SetCharState(CharState.Carrying);
                        }

                        AudioController.PlaySFXOneShot("SFX_QueenWork_PickUp");
                        InputMgr.SetRumble(TypeSimpleRumble.Medium);
                        return;
                    }

                    GameMgr.Instance._CenterAlarmUI.CenterAlarmSet(C_AlarmState.Hand_Full);
                    if (GameMgr.Instance._EditorMgr.m_TutoNpc && GameMgr.Instance._NpcAlarmUI.Arr_Checker[2] == 0 && GameMgr.Instance._NpcAlarmUI.MayI_Stop())
                    {
                        GameMgr.Instance._NpcAlarmUI.Arr_Checker[2] = 1;
                        GameMgr.Instance._NpcAlarmUI.NpcAlarm_Call(NpcAlarm_State.DropTuto0, _pause: true);
                        GameMgr.Instance._NpcAlarmUI.NpcAlarm_Call(NpcAlarm_State.DropTuto1, _pause: true);
                    }
                }

                //携带物品时
                if (__instance.m_CharState == CharState.Carrying)
                {
                    if (InputMgr.GetKeyDown(HotKeyName.Interaction))
                    {
                        //仓库
                        if (__instance.m_CheckBox.m_Building != null && __instance.m_CheckBox.m_Building.m_Info.Ability == BuildAbility.Store && __instance.m_CheckBox.AreYouSelectType(MiniType.Building))
                        {
                            if (__instance.m_CheckBox.m_Building.m_BuildState != 0)
                            {
                                GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Building is inactive"), Color.red);
                                return;
                            }

                            if (__instance.List_Gathering.Count > 0 && __instance.List_Gathering[0].m_Type == TileType.A_Citizen)
                            {
                                GameMgr.Instance._CenterAlarmUI.CenterAlarmCustomSet(LocalizationManager.GetTranslation("Alarm/Cant store citizen"), Color.red);
                            }

                            if (!(__instance != null) || !__instance.m_CheckBox.m_Building.DeliveryInteraction(__instance))
                            {
                                return;
                            }

                            if (DefinesGetValue<bool>("IsMouseTargetOn"))
                            {
                                if (__instance.m_CheckBox.m_Building.Tf.position.x > __instance.Tf.position.x)
                                {
                                    __instance.FlipX(right: true);
                                }
                                else
                                {
                                    __instance.FlipX(right: false);
                                }
                            }

                            InputMgr.SetRumble(TypeSimpleRumble.Medium);
                            __instance.SetAniState(AniState.TakeOut, _loop: false, _now: false);
                            __instance.SetCharState(CharState.None);
                            __instance.m_CheckBox.HotKeyUpdate();
                        }
                        //建筑蓝图
                        else if (__instance.m_CheckBox.IsBP_FormEnable() && __instance.m_CheckBox.m_BP_Building.IsNeedMat(__instance.List_Gathering))
                        {
                            if (DefinesGetValue<bool>("IsMouseTargetOn"))
                            {
                                if (__instance.m_CheckBox.m_BP_Building.Tf.position.x > __instance.Tf.position.x)
                                {
                                    __instance.FlipX(right: true);
                                }
                                else
                                {
                                    __instance.FlipX(right: false);
                                }
                            }

                            AudioController.PlaySFXOneShot("SFX_Effect_Grain", GameMgr.Instance._CamMgr.m_MainCam.transform.position, __instance.Tf.position);
                            __instance.m_CheckBox.BP_BuildingInteraction();
                            __instance.m_CheckBox.HotKeyUpdate();
                        }
                        //植物蓝图
                        else if (__instance.m_CheckBox.IsBP_PlantEnable() && __instance.m_CheckBox.m_BP_Plant.IsNeedMat(__instance.List_Gathering))
                        {
                            if (DefinesGetValue<bool>("IsMouseTargetOn"))
                            {
                                if (__instance.m_CheckBox.m_BP_Plant.Tf.position.x > __instance.Tf.position.x)
                                {
                                    __instance.FlipX(right: true);
                                }
                                else
                                {
                                    __instance.FlipX(right: false);
                                }
                            }

                            AudioController.PlaySFXOneShot("SFX_Effect_Grain", GameMgr.Instance._CamMgr.m_MainCam.transform.position, __instance.Tf.position);
                            __instance.m_CheckBox.BP_PlantInteraction();
                            __instance.m_CheckBox.HotKeyUpdate();
                        }
                        //市民
                        else if (__instance.m_CheckBox.AreYouRealSelectType(MiniType.Citzien))
                        {
                            GameUnit selectGameUnit2 = __instance.m_CheckBox.GetSelectGameUnit();
                            if ((object)selectGameUnit2 != null && selectGameUnit2.m_E_Name == EnemyType.Shrew)
                            {
                                __instance.m_CheckBox.GetSelectGameUnit().GetComponent<Shrew>().AddGatherItem();
                                return;
                            }
                        }
                    }
                    //丢弃物品
                    else if (InputMgr.GetKeyDown(HotKeyName.DropObj) && __instance.m_AniState == AniState.Idle)
                    {
                        __instance.DropAction();
                        return;
                    }
                }
                else if ((__instance.m_CharState == CharState.Mining || __instance.m_CharState == CharState.Gathering || __instance.m_CharState == CharState.Building || __instance.m_CharState == CharState.Planting) && InputMgr.GetKeyUp(HotKeyName.Interaction))
                {
                    __instance.BehaviorStop();
                    return;
                }

                /*跳跃相关
                if (__instance.m_AniState != AniState.DashJump && __instance.m_AniState != AniState.Falling1 && __instance.m_AniState != AniState.ClimbJump && __instance.m_AniState != AniState.GrabToWall && __instance.m_AniState != AniState.DropToGround && __instance.m_AniState != AniState.FallToLadder && __instance.m_AniState != AniState.Falling2)
                {
                    //跳跃
                    if (InputMgr.GetKey(HotKeyName.Jump))
                    {
                        if (InputMgr.GetKey(HotKeyName.LeftDir) && !InputMgr.GetKey(HotKeyName.RightDir))
                        {
                            if (InputMgr.GetKey(HotKeyName.UpDir) && GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Left_Grab, m_MiniUnitInfo))
                            {
                                Move(MoveDir.Left_Grab, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x - 1, __instance.m_CurNode.y + 2));
                                return;
                            }

                            if (InputMgr.GetKey(HotKeyName.BottomDir) && GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Left_DownJump, m_MiniUnitInfo))
                            {
                                Move(MoveDir.Left_DownJump, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x - 2, __instance.m_CurNode.y - 1));
                                return;
                            }

                            if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Left_Up, m_MiniUnitInfo))
                            {
                                Move(MoveDir.Left_Up, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x - 1, __instance.m_CurNode.y + 1));
                                return;
                            }

                            if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Left_UpJump, m_MiniUnitInfo))
                            {
                                Move(MoveDir.Left_UpJump, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x - 2, __instance.m_CurNode.y + 1));
                                return;
                            }

                            if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Left_Jump, m_MiniUnitInfo))
                            {
                                Move(MoveDir.Left_Jump, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x - 2, __instance.m_CurNode.y));
                                return;
                            }

                            if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Left_Grab, m_MiniUnitInfo))
                            {
                                Move(MoveDir.Left_Grab, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x - 1, __instance.m_CurNode.y + 2));
                                return;
                            }

                            if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Left_DownJump, m_MiniUnitInfo))
                            {
                                Move(MoveDir.Left_DownJump, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x - 2, __instance.m_CurNode.y - 1));
                                return;
                            }
                        }
                        else if (InputMgr.GetKey(HotKeyName.RightDir) && !InputMgr.GetKey(HotKeyName.LeftDir))
                        {
                            if (InputMgr.GetKey(HotKeyName.UpDir) && GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Right_Grab, m_MiniUnitInfo))
                            {
                                Move(MoveDir.Right_Grab, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x + 1, __instance.m_CurNode.y + 2));
                                return;
                            }

                            if (InputMgr.GetKey(HotKeyName.BottomDir) && GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Right_DownJump, m_MiniUnitInfo))
                            {
                                Move(MoveDir.Right_DownJump, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x + 2, __instance.m_CurNode.y - 1));
                                return;
                            }

                            if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Right_Up, m_MiniUnitInfo))
                            {
                                Move(MoveDir.Right_Up, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x + 1, __instance.m_CurNode.y + 1));
                                return;
                            }

                            if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Right_UpJump, m_MiniUnitInfo))
                            {
                                Move(MoveDir.Right_UpJump, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x + 2, __instance.m_CurNode.y + 1));
                                return;
                            }

                            if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Right_Jump, m_MiniUnitInfo))
                            {
                                Move(MoveDir.Right_Jump, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x + 2, __instance.m_CurNode.y));
                                return;
                            }

                            if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Right_Grab, m_MiniUnitInfo))
                            {
                                Move(MoveDir.Right_Grab, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x + 1, __instance.m_CurNode.y + 2));
                                return;
                            }

                            if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Right_DownJump, m_MiniUnitInfo))
                            {
                                Move(MoveDir.Right_DownJump, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x + 2, __instance.m_CurNode.y - 1));
                                return;
                            }
                        }
                    }

                    if (__instance.m_AniState != AniState.Falling_Walking && __instance.m_AniState != AniState.JumpToLadder && __instance.m_AniState != AniState.FallToLadder)
                    {
                        if (InputMgr.GetKey(HotKeyName.UpDir))
                        {
                            if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Up, m_MiniUnitInfo) && GameMgr.Instance._TileMgr.GetNode(__instance.m_CurNode.x, __instance.m_CurNode.y + 1) != m_TargetNode)
                            {
                                Move(MoveDir.Up, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x, __instance.m_CurNode.y + 1));
                                return;
                            }
                        }
                        else if (InputMgr.GetKey(HotKeyName.BottomDir) && !InputMgr.GetKey(HotKeyName.LeftDir) && !InputMgr.GetKey(HotKeyName.RightDir) && GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Down, m_MiniUnitInfo) && GameMgr.Instance._TileMgr.GetNode(__instance.m_CurNode.x, __instance.m_CurNode.y - 1) != m_TargetNode)
                        {
                            Move(MoveDir.Down, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x, __instance.m_CurNode.y - 1));
                            return;
                        }
                    }

                    if (__instance.m_AniState != AniState.Walking_UpDown && __instance.m_AniState != AniState.Ladder_Up && __instance.m_AniState != AniState.Ladder_Down)
                    {
                        if (InputMgr.GetKey(HotKeyName.LeftDir) && InputMgr.GetKey(HotKeyName.BottomDir) && !InputMgr.GetKey(HotKeyName.RightDir) && __instance.m_MoveDir != MoveDir.Left_Drop && __instance.m_MoveDir != MoveDir.Right_Down && __instance.m_MoveDir != MoveDir.Left_Down && __instance.m_AniState != AniState.JumpToLadder && __instance.m_AniState != AniState.FallToLadder)
                        {
                            m_KeyDownTime += Time.deltaTime;
                            if (m_KeyDownTime > 0.01f)
                            {
                                NodeUpdate();
                                if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Left_Drop, m_MiniUnitInfo) && (m_TargetNode.x != __instance.m_CurNode.x - 1 || m_TargetNode.y != __instance.m_CurNode.y - 2))
                                {
                                    Move(MoveDir.Left_Drop, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x - 1, __instance.m_CurNode.y - 2));
                                    return;
                                }
                            }
                            else if (!IsMoveState())
                            {
                                __instance.FlipX(right: false);
                            }
                        }

                        if (InputMgr.GetKey(HotKeyName.RightDir) && InputMgr.GetKey(HotKeyName.BottomDir) && !InputMgr.GetKey(HotKeyName.LeftDir) && __instance.m_MoveDir != MoveDir.Right_Drop && __instance.m_MoveDir != MoveDir.Right_Down && __instance.m_MoveDir != MoveDir.Left_Down && __instance.m_AniState != AniState.JumpToLadder && __instance.m_AniState != AniState.FallToLadder)
                        {
                            m_KeyDownTime += Time.deltaTime;
                            if (m_KeyDownTime > 0.01f)
                            {
                                NodeUpdate();
                                if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Right_Drop, m_MiniUnitInfo) && (m_TargetNode.x != __instance.m_CurNode.x + 1 || m_TargetNode.y != __instance.m_CurNode.y - 2))
                                {
                                    Move(MoveDir.Right_Drop, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x + 1, __instance.m_CurNode.y - 2));
                                    return;
                                }
                            }
                            else if (!IsMoveState())
                            {
                                __instance.FlipX(right: false);
                            }
                        }

                        if (InputMgr.GetKey(HotKeyName.LeftDir) && !InputMgr.GetKey(HotKeyName.RightDir) && __instance.m_MoveDir != MoveDir.Left_Drop && __instance.m_MoveDir != MoveDir.Right_Drop && __instance.m_MoveDir != MoveDir.Up && __instance.m_AniState != AniState.Jump && __instance.m_MoveDir != MoveDir.Right_Down && __instance.m_MoveDir != MoveDir.Left_Down && __instance.m_AniState != AniState.JumpToLadder && __instance.m_AniState != AniState.FallToLadder)
                        {
                            m_KeyDownTime += Time.deltaTime;
                            if (m_KeyDownTime > 0.01f)
                            {
                                NodeUpdate();
                                if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Left, m_MiniUnitInfo) && (m_TargetNode.x != __instance.m_CurNode.x - 1 || m_TargetNode.y != __instance.m_CurNode.y))
                                {
                                    Move(MoveDir.Left, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x - 10, __instance.m_CurNode.y));
                                    UpdateSideMove();
                                }
                                else if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Left_Down, m_MiniUnitInfo) && (m_TargetNode.x != __instance.m_CurNode.x - 1 || m_TargetNode.y != __instance.m_CurNode.y - 1))
                                {
                                    Move(MoveDir.Left_Down, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x - 1, __instance.m_CurNode.y - 1));
                                }
                                else if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Left_Drop, m_MiniUnitInfo) && (m_TargetNode.x != __instance.m_CurNode.x - 1 || m_TargetNode.y != __instance.m_CurNode.y - 2))
                                {
                                    Move(MoveDir.Left_Drop, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x - 1, __instance.m_CurNode.y - 2));
                                }
                                else
                                {
                                    __instance.FlipX(right: false);
                                }
                            }
                            else if (!IsMoveState())
                            {
                                __instance.FlipX(right: false);
                            }

                            return;
                        }

                        if (InputMgr.GetKey(HotKeyName.RightDir) && !InputMgr.GetKey(HotKeyName.LeftDir) && __instance.m_MoveDir != MoveDir.Right_Drop && __instance.m_MoveDir != MoveDir.Left_Drop && __instance.m_MoveDir != MoveDir.Up && __instance.m_AniState != AniState.Jump && __instance.m_MoveDir != MoveDir.Right_Down && __instance.m_MoveDir != MoveDir.Left_Down && __instance.m_AniState != AniState.JumpToLadder && __instance.m_AniState != AniState.FallToLadder)
                        {
                            m_KeyDownTime += Time.deltaTime;
                            if (m_KeyDownTime > 0.01f)
                            {
                                NodeUpdate();
                                if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Right, m_MiniUnitInfo) && (m_TargetNode.x != __instance.m_CurNode.x + 1 || m_TargetNode.y != __instance.m_CurNode.y))
                                {
                                    Move(MoveDir.Right, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x + 10, __instance.m_CurNode.y));
                                    UpdateSideMove();
                                }
                                else if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Right_Down, m_MiniUnitInfo) && (m_TargetNode.x != __instance.m_CurNode.x + 1 || m_TargetNode.y != __instance.m_CurNode.y - 1))
                                {
                                    GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Right, m_MiniUnitInfo);
                                    Move(MoveDir.Right_Down, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x + 1, __instance.m_CurNode.y - 1));
                                }
                                else if (GameMgr.Instance._PathFindMgr.CanIMoveNode(__instance.m_CurNode, MoveDir.Right_Drop, m_MiniUnitInfo) && (m_TargetNode.x != __instance.m_CurNode.x + 1 || m_TargetNode.y != __instance.m_CurNode.y - 2))
                                {
                                    Move(MoveDir.Right_Drop, GameMgr.Instance._TileMgr.GetNodeByLimit(__instance.m_CurNode.x + 1, __instance.m_CurNode.y - 2));
                                }
                                else
                                {
                                    __instance.FlipX(right: true);
                                }
                            }
                            else if (!IsMoveState())
                            {
                                __instance.FlipX(right: true);
                            }

                            return;
                        }

                        if (InputMgr.GetKey(HotKeyName.LeftDir) || (InputMgr.GetKey(HotKeyName.RightDir) && InputMgr.GetKey(HotKeyName.UpDir)))
                        {
                            if ((__instance.m_CharState == CharState.None || __instance.m_CharState == CharState.Carrying) && __instance.m_AniState != 0 && m_Spine.AnimationState.GetCurrent(0).IsComplete && !IsMoveState())
                            {
                                __instance.SetAniState(AniState.Idle, _loop: true, _now: false);
                            }

                            return;
                        }
                    }

                    if ((__instance.m_CharState == CharState.None || __instance.m_CharState == CharState.Carrying) && __instance.m_AniState != 0 && m_Spine.AnimationState.GetCurrent(0).IsComplete)
                    {
                        if (!IsMoveState())
                        {
                            __instance.SetAniState(AniState.Idle, _loop: true, _now: false);
                        }
                    }
                    else if (__instance.m_CharState == CharState.None && __instance.m_AniState == AniState.Idle && InputMgr.GetKeyDown_F(HotKeyName.Jump))
                    {
                        __instance.SetAniState(AniState.Jump, m_WeaponOn ? "BattleJump" : "Jump", _loop: false, _now: false);
                    }
                }

                m_KeyDownTime = 0f;
                */
            }
        }

        #endregion

        #region 无人机翻倍

        /// <summary>
        /// 女王丢弃物品前
        /// 来源为建造事件时，标记跳过丢弃
        /// </summary>
        public static void Building_MiningCompany_LoadSetting3(Building_MiningCompany __instance)
        {
            ResetMiningCompanyDroneCount(__instance, ActiveAddDroneCount ? 6 : 3);
        }

        /// <summary>
        /// 重置机库无人机的数量
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="count"></param>
        static void ResetMiningCompanyDroneCount(Building_MiningCompany __instance, int count = 6)
        {
            List<MiningDrone> droneList = GetPrivateValue<List<MiningDrone>>(__instance, "List_Drone");

            if (droneList.Count == count)
                return;

            //销毁所有出口的飞机
            for (int i = 0; i < droneList.Count; i++)
            {
                if (UnitMgr.List_MDrone.Contains(droneList[i]))
                    UnitMgr.List_MDrone.Remove(droneList[i]);

                droneList[i].DeathCheck(0);
            }

            droneList.Clear();

            //每个出口轮流生成指定数量的飞机
            for (int j = 0; j < count; j++)
            {
                MiningDrone component = GameMgr.Instance._PoolMgr.Pool_MiningDrone.GetNextObj().GetComponent<MiningDrone>();

                droneList.Add(component);

                droneList[j].MakeMiningDrone(__instance.GetDroneHomePos(j % 3), __instance, j);

                droneList[j].Obj.SetActive(false);

                droneList[j].RunTypeSet((int)__instance.m_DroneActInfo.m_Option);
            }

            //立即刷新无人机行动
            __instance.Building_Update3();

            //Debug.Log($"{__instance.m_Info.T_Name} 已经重置为 {droneList.Count} 架无人机");
        }

        #endregion

        #region 机器人翻倍

        static int DefaultBotCount = 30;

        /// <summary>
        /// 加载默认机器人数量限制
        /// </summary>
        static void LoadDefaultBotCount()
        {
            DefaultBotCount = DBMgr.Dic_BuildDB[BuildingName.RatronFactory].EffectValue2_Num;
        }

        #endregion

        #region 自定义皮肤

        /// <summary>
        /// 市民可自定义的皮肤部位名称列表
        /// </summary>
        static readonly string[] CitizenCustomSkinCategoryNames = new string[] { "Basic", "Skin", "Face", "Bread", "Hair", "Makeup", "Glasses", "Dress", "Hat" };
        static readonly string[] CitizenCustomSkinCategoryNames_CN = new string[] { "预制", "肤色", "面貌", "胡须", "发型" ,"化妆", "眼镜", "衣服", "帽子" };
        /// <summary>
        /// 自定义部位默认值
        /// </summary>
        static readonly Dictionary<string, string> CustomCategoryDefaultValues = new Dictionary<string, string>()
        {
            { "Face", "Face_1" }, { "Skin", "White" }, { "Hair_Male", "Hair_1" }, { "Hair_Female", "Hair_2" }, { "Dress_Male", "Dress_29" }, { "Dress_Female", "Dress_28" }
        };
        /// <summary>
        /// 市民可自定义的部位皮肤
        /// </summary>
        static Dictionary<string, List<string>> CitizenCustomCategorySkins = new Dictionary<string, List<string>>();

        /// <summary>
        /// 特殊鼠鼠的皮肤
        /// </summary>
        static Dictionary<string, Dictionary<string, string>> SpecialCitizenSkins = new Dictionary<string, Dictionary<string, string>>();
        /// <summary>
        /// 市民的自定义皮肤
        /// </summary>
        static Dictionary<int, Dictionary<string, string>> CitizenCustomSkins = new Dictionary<int, Dictionary<string, string>>();
        /// <summary>
        /// 已打开的市民信息
        /// </summary>
        static T_Citizen OpenedCitizenInfo = null;
        /// <summary>
        /// 打开了特殊鼠鼠的信息
        /// </summary>
        static bool OpenedSpcialCitizen = false;
        /// <summary>
        /// 在编辑的皮肤信息，<部位名，皮肤名>
        /// </summary>
        static Dictionary<string, string> EditingCustomSkins = null;
        /// <summary>
        /// 在编辑的自定义皮肤下标，<部位名，所选下标>
        /// </summary>
        static Dictionary<string, int> EditingCustomSkinIndex = new Dictionary<string, int>();
        /// <summary>
        /// 当前打开的市民有预制皮肤
        /// </summary>
        static bool OpenedCitizenHavePremanentSkin { get { return OpenedCitizenInfo != null && EditingCustomSkinIndex.TryGetValue("Basic", out int value) && value != 0; } }

        /// <summary>
        /// 显示市民信息界面
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static void CitizenInfoUI_Show(T_Citizen citizen)
        {
            if (!ActiveCustomCitizenSkin)
                return;

            OpenedCitizenInfo = citizen;

            OpenedSpcialCitizen = CitizenIsSpecialUnit(citizen.m_UnitName, citizen.m_ID, out _);

            if (OpenedSpcialCitizen)
                SpecialCitizenSkins.TryGetValue(citizen.m_UnitName, out EditingCustomSkins);
            else
                TryGetCitizenCustomSkin(citizen.m_ID, out EditingCustomSkins);

            //获取市民使用的皮肤下标
            foreach (KeyValuePair<string, string> skin in EditingCustomSkins)
            {
                if (!CitizenCustomCategorySkins.TryGetValue(skin.Key, out List<string> customSkinNames))
                {
                    Debug.LogError($"自定义皮肤部位 {skin.Key} 获取失败！");

                    continue;
                }

                int index = customSkinNames.IndexOf(skin.Value.Trim());

                EditingCustomSkinIndex[skin.Key] = index == -1 ? 0 :index;

                if (index == -1)
                {
                    foreach (string name in customSkinNames)
                    {
                        Debug.Log($"{name} - {skin.Value.Trim()}，Equal : {name.Equals(skin.Value)}，TrimEqual : {name.Equals(skin.Value.Trim())}");
                    }

                    Debug.LogError($"部位 {skin.Key} 获取皮肤 {skin.Value} 失败！");
                }
                else
                    Debug.LogWarning($"市民 {citizen.m_UnitName} 部位 {skin.Key} 的皮肤是 {(skin.Value.Equals("") ? "未指定" : skin.Value)}");
            }

            Debug.Log($"已打开市民 [{citizen.m_ID}]{citizen.m_UnitName} 的信息界面，EditingCustomSkins {EditingCustomSkins.Count}");
        }
        
        /// <summary>
         /// 隐藏市民信息界面
         /// </summary>
         /// <param name="__instance"></param>
         /// <param name="num"></param>
         /// <returns></returns>
        public static void CitizenInfoUI_Hide()
        {
            if (!ActiveCustomCitizenSkin)
                return;

            SaveCustomSkinSetting(GameData);

            OpenedSpcialCitizen = false;

            OpenedCitizenInfo = null;

            EditingCustomSkins = null;

            //UpdateFromCustomSkinSet = false;
        }

        /// <summary>
        /// 市民更新默认服装前
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool T_Citizen_DefaultClothesUpdate(T_Citizen __instance)
        {
            if (!UpdateClothes(__instance))
                return true;

            Debug.Log($"单位 {__instance.m_UnitName} 更新了默认服装");

            return false;
        }

        /// <summary>
        /// 单位更新服装前
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool GameUnit_ClothesUpdate(GameUnit __instance, int num)
        {
            //非工作时
            if (num != 0 || !UpdateClothes(__instance))
                return true;

            //Debug.Log($"单位 {__instance.m_UnitName} 更新了服装");

            return false;
        }

        /// <summary>
        /// 更新服装
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        static bool UpdateClothes(GameUnit unit)
        {
            if (!CitizenIsSpecialUnit(unit.m_UnitName, unit.m_ID, out T_Citizen citizen) ||
                !TryGetSpecialUnit(citizen, out CustomSpecialUnit specialUnit) ||
                !SpecialCitizenSkins.TryGetValue(citizen.m_UnitName, out Dictionary<string, string> customSkin))
                return false;

            citizen.m_SkinInfo.m_Gender = citizen.m_Gender;

            bool updated = UpdateUnitSpineDress(
                citizen.m_SkinInfo,
                citizen.m_UnitName,
                citizen.m_Gender.ToString(),
                citizen.m_Job,
                true,
                customSkin);

            Debug.Log($"特殊单位 {specialUnit.Name} {(updated ? "更新" : "恢复")}了服装");
            return updated;
        }

        /// <summary>
        /// 设置编辑中的自定义皮肤
        /// </summary>
        /// <param name="skins"></param>
        /// <param name="category"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        void SetEditingCustomSkin(List<string> skins, string category, int index = -100, int length = -1)
        {
            if (index != -100)
                index = GetCircularIndex(length, index);
            else
                index = 0;

            EditingCustomSkinIndex[category] = index;

            EditingCustomSkins[category] = skins[index];

            UpdateUnitSpineDress(OpenedCitizenInfo.m_SkinInfo, OpenedCitizenInfo.m_ID.ToString(), OpenedCitizenInfo.m_Gender.ToString(), OpenedCitizenInfo.m_Job, true, EditingCustomSkins);
        }

        /// <summary>
        /// 获得循环索引
        /// </summary>
        /// <param name="length"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        int GetCircularIndex(int length, int value)
        {
            return value == length ? 0 : value < 0 ? length - 1 : value;
        }

        /// <summary>
        /// 尝试获取市民自定义皮肤
        /// </summary>
        /// <param name="id"></param>
        /// <param name="customSkin"></param>
        /// <returns></returns>
        static bool TryGetCitizenCustomSkin(int id, out Dictionary<string, string> customSkin)
        {
            //获取市民的皮肤信息
            if (!CitizenCustomSkins.TryGetValue(id, out customSkin))
            {
                customSkin = new Dictionary<string, string>(CitizenCustomCategorySkins.Count);

                //初始所有部位的皮肤为未指定状态
                foreach (string key in CitizenCustomCategorySkins.Keys)
                {
                    customSkin[key] = "";
                }

                CitizenCustomSkins[id] = customSkin;

                Debug.Log($"初始化 {id} 的皮肤");

                return false;
            }
            //皮肤部位的数量与可自定义的数量不同时（如版本更新）
            else if (customSkin.Count != CitizenCustomCategorySkins.Count)
            {
                //将不存在的部位重置为空
                foreach (string key in CitizenCustomCategorySkins.Keys)
                {
                    if (customSkin.ContainsKey(key))
                        continue;

                    customSkin[key] = "";
                }
            }

            return true;
        }

        /// <summary>
        /// 注册自定义皮肤
        /// </summary>
        /// <param name="unit"></param>
        static void RegisterCustomSkin(Sp_SkinInfo skinInfo, CustomSpecialUnit unit, bool applyToSkeleton)
        {
            string key = unit.Name;
            string gender = skinInfo.m_Gender.ToString();

            SpecialCitizenSkins[key] = new Dictionary<string, string>() { { "Skin", unit.skin.Trim() }, { "Face", unit.face.Trim() }, { "Bread", unit.bread.Trim() }, { "Dress", unit.dress.Trim() }, { "Glasses", unit.glasses.Trim() }, { "Hair", unit.hair.Trim() }, { "Hat", unit.hat.Trim() }, { "Makeup", unit.makeup.Trim() } };

            Debug.Log($"特殊皮肤 {key} 注册：实际模板性别 {gender}");

            UpdateUnitSpineDress(skinInfo, key, gender, null, true, SpecialCitizenSkins[key], applyToSkeleton);
        }

        /// <summary>
        /// 更新单位皮肤
        /// </summary>
        /// <param name="skinInfo"></param>
        /// <param name="key"></param>
        /// <param name="gender"></param>
        /// <param name="job"></param>
        /// <param name="isCitizen"></param>
        /// <param name="customSkin"></param>
        static bool UpdateUnitSpineDress(Sp_SkinInfo skinInfo, string key, string gender, Building job, bool isCitizen, Dictionary<string, string> customSkin, bool applyToSkeleton = true)
        {
            SpineDresserBundle bundle = SpineDresserMgr.Instance.Bundle;

            bool hasKey = bundle.HasKey(key);

            bool havePermanent = customSkin.TryGetValue("Basic", out string value) && !value.Equals("");

            TryGetJobPairs(job, gender, out SpineDresserPair[] jobPairs);

            //皮肤元素：模版->部位组合
            SpineDresserElement element = SpineDresserElement.Create(key);

            //模版：男/女
            SpineDresserTemplete templete = SpineDresserTemplete.Create(gender);

            templete.Pairs = new SpineDresserPair[customSkin.Count];

            int index = 0;

            foreach (KeyValuePair<string, string> customPair in customSkin)
            {
                //当设置了预设皮肤时，其他皮肤显示为未设置状态
                string pair = SkinPairCorrection(gender, customPair.Key, customPair.Value, havePermanent, jobPairs);

                templete.Pairs[index] = SpineDresserPair.Create(customPair.Key, pair);

                //Debug.Log($"{key} 更新了部位[{index}] {customPair.Key} 为 {pair} <= {customPair.Value} ");

                index++;
            }

            SetStructPrivateValue(ref element, "_templetes", new SpineDresserTemplete[] { templete });

            bool isAdd = false;

            //更新皮肤
            if (hasKey)
            {
                //迭代所有皮肤
                for (int i = 0; i < bundle.Elements.Length; i++)
                {
                    if (bundle.Elements[i].Key.Equals(element.Key))
                    {
                        bundle.Elements[i] = element;

                        break;
                    }
                }

                Debug.LogWarning($"{key} 更新了皮肤");

                //SetPrivateValue(bundle, "_elements", bundle.Elements);
            }
            //添加皮肤
            else
            {
                isAdd = true;

                SetPrivateValue(bundle, "_elements", bundle.Elements.AddToArray(element));
            }
            
            Debug.Log($"{key} {(isAdd ? "添加" : "更新")}了皮肤，当前共有 {bundle.Elements.Length} 个皮肤");

            // && !CitizenCaveUI.Obj_Main.activeSelf

            return UpdateUnitCustomSkin(skinInfo, key, isCitizen, applyToSkeleton);
        }

        /// <summary>
        /// 部位皮肤修正
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="havePermanent"></param>
        /// <returns></returns>
        static string SkinPairCorrection(string gender, string key, string value, bool havePermanent, SpineDresserPair[] jobPairs)
        {
            //预制皮肤使用预制配置
            if (havePermanent)
                return key.Equals("Basic") ? value : "";

            if (!value.Equals(""))
                return value;

            //使用职业皮肤配置
            if (jobPairs != null)
            {
                foreach (SpineDresserPair pair in jobPairs)
                {
                    //存在该部位且有配置时
                    if (pair.Category.Equals(key) && !pair.Skin.Equals(""))
                    {
                        Debug.LogWarning($"部位 {pair.Category} 使用了职业皮肤 {pair.Skin}");

                        return pair.Skin;
                    }
                }
            }

            if (key.Equals("Hair"))
                key = $"Hair_{gender}";

            if (key.Equals("Dress"))
                key = $"Dress_{gender}";

            return CustomCategoryDefaultValues.TryGetValue(key, out string corValue) ? corValue : "";
        }

        /// <summary>
        /// 获取职业皮肤配置
        /// </summary>
        /// <param name="job"></param>
        /// <param name="gender"></param>
        /// <param name="jobPairs"></param>
        /// <returns></returns>
        static bool TryGetJobPairs(Building job, string gender, out SpineDresserPair[] jobPairs)
        {
            jobPairs = null;

            if (job == null)
                return false;

            string jobKey = job.m_Info.Name.ToString();

            if (job.m_Info.Ability == BuildAbility.Barrack && job.m_BuildInfoUI.IsProductEnable())
            {
                MilitaryInfo militaryInfo = GameMgr.Instance._DB_Mgr.m_MilitaryDB._list.Find((MilitaryInfo x) => x.Index == job.m_BuildInfoUI.GetProductIndex());

                if (militaryInfo != null)
                    jobKey = militaryInfo.SkinName;
            }

            return TryGetSpineDresserElement(jobKey, out SpineDresserElement jobSkin) && jobSkin.TryGetPairs(gender, out jobPairs);
        }

        /// <summary>
        /// 尝试获取皮肤
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        static bool TryGetSpineDresserElement(string key, out SpineDresserElement result)
        {
            List<SpineDresserElement> list = SpineDresserMgr.Instance.Bundle.Elements.Where(t => t.Key.Equals(key)).ToList();

            result = list.FirstOrDefault();

            return list.Count > 0;
        }

        /// <summary>
        /// 更新单位自定义皮肤
        /// </summary>
        /// <param name="skinInfo"></param>
        /// <param name="key"></param>
        /// <param name="isCitizen"></param>
        static bool UpdateUnitCustomSkin(Sp_SkinInfo skinInfo, string key, bool isCitizen = true, bool applyToSkeleton = true)
        {
            Dictionary<string, string> skinSnapshot = new Dictionary<string, string>(skinInfo.SkinDic);
            Dictionary<string, string> overrideSnapshot = new Dictionary<string, string>(skinInfo.OverrideSkinDic);
            string gender = skinInfo.m_Gender.ToString();

            if (!TryGetSpineDresserElement(key, out SpineDresserElement element) ||
                !element.TryGetPairs(gender, out SpineDresserPair[] pairs) ||
                pairs == null)
            {
                if (isCitizen)
                    RecoverUnitSkin(skinInfo, skinSnapshot, overrideSnapshot, key, $"缺少 {gender} 模板", applyToSkeleton);
                else
                    Debug.LogError($"特殊皮肤 {key} 组合失败：缺少 {gender} 模板");

                return false;
            }

            if (isCitizen)
                skinInfo.ClearSkins();

            SpineDresserMgr.Instance.AssembleData(key, skinInfo, false);

            if (isCitizen && !SkinRepairPolicy.HasRequiredAppearance(skinInfo.SkinDic))
            {
                string missing = string.Join(",", SkinRepairPolicy.MissingRequiredCategories(skinInfo.SkinDic));
                RecoverUnitSkin(skinInfo, skinSnapshot, overrideSnapshot, key, $"缺少关键部件 {missing}", applyToSkeleton);
                return false;
            }

            RenderCombinedSkin(skinInfo, applyToSkeleton);
            return true;
        }

        /// <summary>
        /// 特殊皮肤更新失败时恢复可见外观。
        /// </summary>
        static void RecoverUnitSkin(Sp_SkinInfo skinInfo, Dictionary<string, string> skinSnapshot, Dictionary<string, string> overrideSnapshot, string key, string reason, bool applyToSkeleton)
        {
            SkinRecoveryKind recovery = SkinRepairPolicy.SelectRecovery(skinSnapshot);

            skinInfo.ClearSkins();
            skinInfo.ClearOverrideSkin();

            if (recovery == SkinRecoveryKind.Snapshot)
            {
                skinInfo.SetStyles(skinSnapshot, null);

                foreach (KeyValuePair<string, string> pair in overrideSnapshot)
                    skinInfo.SetStyleOverride(pair.Key, pair.Value);
            }
            else
            {
                SpineDresserMgr.Instance.AssembleDefaultSkin(skinInfo);
                SpineDresserMgr.Instance.AssembleData("Jobless_1_1", skinInfo, true);
            }

            RenderCombinedSkin(skinInfo, applyToSkeleton);

            string recoveryName = recovery == SkinRecoveryKind.Snapshot ? "原外观" : "原版默认外观";
            Debug.LogError($"特殊皮肤 {key} 组合失败：{reason}；已使用 {recoveryName} 恢复");

            if (!SkinRepairPolicy.HasRequiredAppearance(skinInfo.SkinDic))
            {
                string missing = string.Join(",", SkinRepairPolicy.MissingRequiredCategories(skinInfo.SkinDic));
                Debug.LogError($"特殊皮肤 {key} 恢复后仍缺少关键部件：{missing}");
            }
        }

        /// <summary>
        /// 将组合皮肤安装到当前 Spine 骨架。
        /// </summary>
        static void RenderCombinedSkin(Sp_SkinInfo skinInfo, bool applyToSkeleton)
        {
            skinInfo.SkinSet(skinInfo.m_Skin, skinInfo.m_SkeletonData);

            if (applyToSkeleton)
                skinInfo.UpdateCombinedSkin();
        }

        /// <summary>
        /// 保存自定义皮肤设定
        /// </summary>
        /// <param name="dData"></param>
        static void SaveCustomSkinSetting(D_Data dData)
        {
            string path = $"{SavePath}/{dData.DirectoryName}/{dData.FileName}.skin";

            bool result = BaseCommand.SaveObjectToJson(path, CitizenCustomSkins);

            Debug.LogWarning($"自定义皮肤保存 {result}，路径 {path}");
        }

        /// <summary>
        /// 加载自定义皮肤设定
        /// </summary>
        /// <param name="dData"></param>
        static void LoadCustomSkinSetting(D_Data dData)
        {
            string path = $"{SavePath}/{dData.DirectoryName}/{dData.FileName}.skin";

            bool result = BaseCommand.LoadObjectByJson(path, out CitizenCustomSkins);

            if (!result)
                CitizenCustomSkins = new Dictionary<int, Dictionary<string, string>>();

            SpecialCitizenSkins.Clear();

            Debug.LogWarning($"自定义皮肤加载 {result}，路径 {path}");
        }

        #endregion

        #region Defines设置

        /// <summary>
        /// 设置Defines值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void DefinesSetValue(string name, object value)
        {
            assembly.GetType("Defines").GetField(name, AccessTools.all).SetValue(null, value);
        }

        /// <summary>
        /// 获得Defines值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T DefinesGetValue<T>(string name)
        {
            return (T)assembly.GetType("Defines").GetField(name, AccessTools.all).GetValue(null);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 获取私有变量值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static T GetPrivateValue<T>(object instance, string fieldName)
        {
            return Traverse.Create(instance).Field(fieldName).GetValue<T>();
        }

        /// <summary>
        /// 设置私有变量值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static void SetPrivateValue<T>(object instance, string fieldName, T value)
        {
            Traverse.Create(instance).Field(fieldName).SetValue(value);
        }

        /// <summary>
        /// 设置结构私有变量值
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="instance"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public static void SetStructPrivateValue<T1, T2>(ref T1 instance, string fieldName, T2 value) where T1 : struct
        {
            object obj = instance;

            Traverse.Create(obj).Field(fieldName).SetValue(value);

            instance = (T1)obj;
        }

        /// <summary>
        /// 物品名称
        /// </summary>
        /// <param name="type"></param>
        /// <param name="withValue"></param>
        /// <returns></returns>
        static string TileName(TileType type, bool withValue = false)
        {
            if (DBMgr.Dic_TileDB.TryGetValue(type, out TileInfo info))
                return string.Format("{0}{1}", info.T_Name, withValue ? $"({info.Value})" : "");

            return type.ToString();
        }

        /// <summary>
        /// 添加状态
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="state"></param>
        static void AddCitizenState(T_Citizen citizen, CitizenState state)
        {
            if (!citizen.List_State.Contains(state))
                citizen.List_State.Add(state);
        }

        static TextEditor copyEditor = new TextEditor();
        /// <summary>
        /// 复制文本至剪贴板
        /// </summary>
        /// <param name="value"></param>
        static void CopyTextToClipboard(string value)
        {
            copyEditor.text = value;

            copyEditor.SelectAll();

            copyEditor.Copy();
        }

        /// <summary>
        /// 中间消息
        /// </summary>
        /// <param name="value"></param>
        /// <param name="color"></param>
        static void CenterMessage(string value, Color? color = null)
        {
            CenterAlarmUI.CenterAlarmCustomSet(value, color ?? Color.white);
        }

        /// <summary>
        /// 创建物品
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pos"></param>
        /// <param name="num"></param>
        /// <param name="objState"></param>
        static void CreateTileObj(TileType type, Vector3 pos, int num = 1, TObjState objState = TObjState.Basic, bool fadeSkip = false)
        {
            if (num == 0)
                return;

            PoolMgr.Pool_TileObject.GetNextObj().GetComponent<TileObject>().ObjectInit(type, objState, pos, num, fadeSkip);
        }

        /// <summary>
        /// 获得一个整数概率
        /// </summary>
        static int IntProbability { get { return RandomInt(0, 100); } }
        /// <summary>
        /// 获得一个浮点概率
        /// </summary>
        static float FloatProbability { get { return RandomFloat(0f, 100f); } }
        /// <summary>
        /// 随机int
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        static int RandomInt(int min, int max)
        {
            return Random.Range(min, max);
        }
        /// <summary>
        /// 随机float
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        static float RandomFloat(float min, float max)
        {
            return Random.Range(min, max);
        }

        #endregion

        #region 测试用

        ///// <summary>
        ///// 单位捡起了物品
        ///// </summary>
        ///// <param name="__instance"></param>
        ///// <param name="_unit"></param>
        ///// <param name="pick_ani"></param>
        //[HarmonyPrefix, HarmonyPatch(typeof(TileObject), "ObjectGathered")]
        //public static void TileObject_ObjectGathered(TileObject __instance, GameUnit _unit, bool pick_ani)
        //{
        //    if (!GameMgr.Instance._DB_Mgr.Dic_TileDB.TryGetValue(__instance.m_Info.m_Type, out TileInfo info))
        //        return;

        //    string buildName = _unit.m_TargetBuilding != null ? $"，目标建筑 {_unit.m_TargetBuilding.m_Info.T_Name}" : "";

        //    Debug.Log($"{_unit.m_UnitName} 捡起了 {info.T_Name}({__instance.m_Info.m_Type}) * {_unit.List_Gathering.Count}/{_unit.Get_HandCapacity()}{buildName}");
        //}
        
        #endregion
    }
}

/// <summary>
/// 自定义特性
/// </summary>
class CustomCharInfo
{
    /// <summary>
    /// 状态
    /// </summary>
    public C_Buff c_Buff;

    /// <summary>
    /// 键名
    /// </summary>
    public string name;

    /// <summary>
    /// 中文名
    /// </summary>
    public string t_name;

    /// <summary>
    /// 值1
    /// </summary>
    public float value1;

    /// <summary>
    /// 值2
    /// </summary>
    public float value2;

    /// <summary>
    /// 描述
    /// </summary>
    public string description;

    /// <summary>
    /// 图标地址
    /// </summary>
    public string iconAddress;

    /// <summary>
    /// 图标键名
    /// </summary>
    public string iconKey;

    /// <summary>
    /// 使用的单位
    /// </summary>
    T_Citizen user;
    public T_Citizen User
    {
        get { return user; }
        set
        {
            user = value;

            Debug.Log($"{user.m_UnitName} 启用了特性 {t_name}");
        }
    }

    /// <summary>
    /// 是否已使用
    /// </summary>
    public bool IsActive { get { return user != null; } }

    public void ClearUser()
    {
        user = null;
    }

    public CustomCharInfo(C_Buff c_Buff)
    {
        this.c_Buff = c_Buff;

        name = "";
        t_name = "";
        iconKey = "";
        user = null;
    }
}

/// <summary>
/// 自定义临时信息
/// </summary>
class CustomTempInfo
{
    /// <summary>
    /// int参数组
    /// </summary>
    public int[] intParams = new int[4];

    public CustomTempInfo(int intValue1 = 0, int intValue2 = 0, int intValue3 = 0, int intValue4 = 0)
    {
        intParams[0] = intValue1;
        intParams[1] = intValue2;
        intParams[2] = intValue3;
        intParams[3] = intValue4;
    }
}

/// <summary>
/// 市民需求阈值
/// </summary>
class CitizenDesireThreshold
{
    /// <summary>
    /// 市民
    /// </summary>
    T_Citizen citizen;

    /// <summary>
    /// 阶级
    /// </summary>
    public int Grade { get { return (int)citizen.m_Grade - 1; } }

    /// <summary>
    /// 恢复阈值
    /// </summary>
    public float RestoreThresholdValue { get { return CustomMOD.RestoreThresholdValues[Grade]; } }

    /// <summary>
    /// 恢复目标值
    /// </summary>
    public float RestoreTargetValue { get { return CustomMOD.RestoreTargetValues[Grade]; } }

    /// <summary>
    /// 最大需求值
    /// </summary>
    public float MaxDesireValue { get { return CustomMOD.MaxDesireValues[Grade]; } }

    /// <summary>
    /// 需要恢复的饥饿值
    /// </summary>
    public float NeedRestoreHungerValue { get { return RestoreTargetValue - citizen.m_Hunger; } }

    /// <summary>
    /// 需要恢复饱食
    /// </summary>
    public bool NeedRestoreHunger { get { return citizen.m_Hunger <= 20 || (!restoringFun && !restoringClean && citizen.m_Hunger < (restoringHunger ? RestoreTargetValue : RestoreThresholdValue)); } }
    /// <summary>
    /// 需要恢复娱乐
    /// </summary>
    public bool NeedRestoreFun { get { return !restoringClean && citizen.m_Fun < (restoringFun ? RestoreTargetValue : RestoreThresholdValue); } }
    /// <summary>
    /// 需要恢复卫生
    /// </summary>
    public bool NeedRestoreClean { get { return citizen.m_Cleanliness < (restoringClean ? RestoreTargetValue : RestoreThresholdValue); } }

    /// <summary>
    /// 正在恢复饱食
    /// </summary>
    public bool restoringHunger = false;
    /// <summary>
    /// 正在恢复娱乐
    /// </summary>
    public bool restoringFun = false;
    /// <summary>
    /// 正在恢复卫生
    /// </summary>
    public bool restoringClean = false;

    public CitizenDesireThreshold(T_Citizen citizen)
    {
        this.citizen = citizen;
    }
}
