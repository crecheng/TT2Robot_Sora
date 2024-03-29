﻿using System.Diagnostics.CodeAnalysis;
using System.Drawing;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

[SuppressMessage("Interoperability", "CA1416:验证平台兼容性")]
public static class SkillCalTool
{

    public static int ColNum = -1;
    public static int RowNum = -1;
    private static bool _init = false;

    public static void Init()
    {
        if(_init)
            return;
        var data = SkillCalDataManage.Instance;
        var num = data.GetMaxDicColRow();
        ColNum  = num >> 16;
        RowNum = num & 0xffff;
    }
    
    
    public static void DrawSkill(this SkillCalCore core, string file, string imgPath)
    {
        Init();
        var data = SkillCalDataManage.Instance;
        var blocks= data.GetAllBlock();
        Dictionary<string, ItemData[,]> allData = new Dictionary<string, ItemData[,]>();
        foreach (var block in blocks)
        {
            allData.Add(block,data.GetDicMat(block,ColNum,RowNum));
        }

        int blockWidth = ColNum * 64;
        int blockHeight = RowNum * 64;

        var row = (blocks.Count - 1) / 3 + 1;

        Bitmap bitmap = new Bitmap(blockWidth * 3+40, blockHeight * row+140);
        Graphics g=Graphics.FromImage(bitmap);
        Font font = new Font(FontFamily.GenericMonospace, 15f,FontStyle.Bold);
        Font fontW = new Font(FontFamily.GenericMonospace, 60f,FontStyle.Bold);
        int b = 0;
        
        foreach (var (key,mat) in allData)
        {
            int x = b % 3;
            int y = b / 3;
            for (int j = 0; j < RowNum; j++)
            {
                for (int i = 0; i < ColNum; i++)
                {
                    var d = mat[i, j];
                    if (d != null)
                    {
                        g.DrawRectangle(Pens.Azure, 20+x * blockWidth + i * 64 + 1, 20+y * blockHeight + j * 64 + 1,
                            62, 62);
                        if (File.Exists($"{imgPath}{d.Img}.png"))
                        {
                            g.DrawImage(Image.FromFile($"{imgPath}{d.Img}.png"),
                                20+x * blockWidth + i * 64 + 3, 20+y * blockHeight + j * 64 + 3);
                        }
                        
                        if(core.Point.ContainsKey(d.Id) && core.Point[d.Id]>0)
                        {
                            g.FillRectangle(Brushes.Gray, 20+x * blockWidth + i * 64 + 32, 20+y * blockHeight + j * 64 + 40,
                                31, 23);
                            g.DrawString(core.Point[d.Id].ToString(), font, Brushes.Azure,
                                20+x * blockWidth + i * 64 + 35, 20+y * blockHeight + j * 64 + 41);

                        }
                    }
                }
            }

            b++;
        }
        g.DrawString($"{core.AllSkillPoint}-{String.Join(",",core.Build)}",font,Brushes.Aqua,20,blockHeight * row+20);
        g.DrawString("加点数据还未完成\n请工具人速速联系我的主人：创造城", font, Brushes.Red, 20, blockHeight * row+40);
        bitmap.Save(file);
    }
}