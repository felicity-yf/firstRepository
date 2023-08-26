using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// acdbmgd.dll 包含对ObjectDBXtmAPI的封装。用于在图形文件中对对象进行操作
using Autodesk.AutoCAD.DatabaseServices;//(Database,DBPoint,Line,Spline)
using Autodesk.AutoCAD.Runtime; //(CommandMethodAttribute,RXObject,CommandFlag)
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Geometry; //(Point3d,Line3d,Curve3d)
// using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.LayerManager;


using Autodesk.AutoCAD.ApplicationServices; // (Application,Document)
using Autodesk.AutoCAD.EditorInput;// (Editor,PromptXOptions,PromptXResult)
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.Windows.ToolPalette;
using Autodesk.AutoCAD.Internal.Windows;
using Autodesk.AutoCAD.Internal.Forms;
using CADTOOL;

namespace Demo
{
    public  class DEMO
    {
        public  PromptPointResult GetPoint2(Editor ed, string promptString)
        {
            //声明一个获取点的指示类;
            PromptPointOptions ppo = new PromptPointOptions(promptString);
            ppo.AllowNone = true;  //使回车和空格有效
            return ed.GetPoint(ppo);
        }

        public  PromptPointResult GetPoint(Editor ed, string promptString, Point3d pointBase, params string[] keyWords)
        {
            PromptPointOptions ppo = new PromptPointOptions(promptString);
            ppo.AllowNone = true;
            //添加字符，使相应的字符有效
            for (int i = 0; i < keyWords.Length; i++)
            {
                ppo.Keywords.Add(keyWords[i]);
            }
            //取消系统自动的关键字显示
            ppo.AppendKeywordsToMessage = false;
            //设置基准点;
            ppo.BasePoint = pointBase;
            ppo.UseBasePoint = true;
            return ed.GetPoint(ppo);
        }

        public static ObjectId AddEntityToModelSpace(Database db, Entity ent)
        {
            ObjectId entId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                entId = btr.AppendEntity(ent);
                tr.AddNewlyCreatedDBObject(ent, true);
                tr.Commit();
            }
            return entId;
        }

        public static ObjectId AddLineToModelSpace(Database db, Point3d startPoint, Point3d endPoint)
        {
            ObjectId entId = ObjectId.Null;
            Line line = new Line(startPoint, endPoint);
            return AddEntityToModelSpace(db, line);
        }

        public  void EraseEntity(Database db, ObjectId entId)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ent = entId.GetObject(OpenMode.ForWrite) as Entity;
                ent.Erase();
                tr.Commit();
            }
        }

        private static Point3d GetLineStartPoint(ObjectId lineId)
        {
            Point3d startPoint;
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Line line = lineId.GetObject(OpenMode.ForWrite) as Line;
                startPoint = line.StartPoint;
            }
            return startPoint;
        }

        [CommandMethod("FlagLine")]
        public  void FlagLine()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //声明一个直线的集合对象
            List<ObjectId> lineList = new List<ObjectId>();

            Point3d pointStart = new Point3d(100, 100, 0);
            Point3d pointPre = new Point3d(100, 100, 0);
            PromptPointResult ppr = GetPoint2(ed,"\n指定第一个点:");

            if (ppr.Status == PromptStatus.Cancel) return;
            if (ppr.Status == PromptStatus.None) pointPre = pointStart;
            if (ppr.Status == PromptStatus.OK)
            {
                pointStart = ppr.Value;
                pointPre = pointStart;
            }
            //判断循环是否继续
            bool isC = true;
            while (isC)
            {
                if (lineList.Count > 1)
                {
                    ppr = GetPoint(ed,"\n指定下一点或[闭合(C)/放弃(U)]:", pointPre, new string[] { "C", "U" });
                }
                else
                {
                    ppr = GetPoint(ed,"\n指定下一个点或[放弃(U)]", pointPre, new string[] { "U" });
                }


                Point3d pointNext;
                if (ppr.Status == PromptStatus.Cancel) return;
                if (ppr.Status == PromptStatus.None) return;
                if (ppr.Status == PromptStatus.OK)
                {
                    pointNext = ppr.Value;
                    lineList.Add(AddLineToModelSpace(db,pointPre, pointNext));
                    pointPre = pointNext;
                }
                if (ppr.Status == PromptStatus.Keyword)
                {
                    switch (ppr.StringResult)
                    {
                        case "U":
                            if (lineList.Count == 0)
                            {
                                pointStart = new Point3d(100, 100, 0);
                                pointPre = new Point3d(100, 100, 0);
                                ppr = GetPoint2(ed,"\n指定第一个点:");
                                if (ppr.Status == PromptStatus.Cancel) return;
                                if (ppr.Status == PromptStatus.None) return;
                                if (ppr.Status == PromptStatus.OK)
                                {
                                    pointStart = ppr.Value;
                                    pointPre = pointStart;
                                }

                            }
                            else if (lineList.Count > 0)
                            {
                                int count = lineList.Count;
                                ObjectId lineId = lineList[count - 1];
                                pointPre = GetLineStartPoint(lineId);
                                lineList.RemoveAt(count - 1);
                                EraseEntity(db, lineId);

                            }
                            break;
                        case "C":
                            lineList.Add(AddLineToModelSpace(db,pointPre, pointStart));
                            isC = false;
                            break;
                    }
                }
            }

        }

        [CommandMethod("AddCircleDemo1")]
        public static void AddCircleDemo1()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Point3d center = new Point3d();
            double radius = 0;
            PromptPointResult ppr = ed.GetPoint("\n请指定圆心:");
            if (ppr.Status == PromptStatus.OK)
            {
                center = ppr.Value;
            }
            PromptDistanceOptions pdo = new PromptDistanceOptions("\n请指定圆上的一个点:");
            pdo.BasePoint = center;
            pdo.UseBasePoint = true;
            pdo.AllowNone = true;
            PromptDoubleResult pdr = ed.GetDistance(pdo);
            if (pdr.Status == PromptStatus.OK)
            {
                radius = pdr.Value;
            }
            db.AddCircleToModelSpace(center, radius);



        }


    }
}
