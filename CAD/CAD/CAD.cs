using System;
using System.IO;
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
using System.Threading;


using AcadApp = Autodesk.AutoCAD.Windows;
using Wnd = System.Windows.Forms;

namespace CAD
{

    public class CAD
    {
        #region
        [CommandMethod("TestDemo")]
        public static void TestDemo()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("这是一条消息");
        }

        [CommandMethod("AddLine")]
        public static void AddLine()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Point3d p1 = new Point3d(0, 200, 0);
            Point3d p2 = new Point3d(600, 800, 0);

            Line line1 = new Line(p1, p2);

            //声明图形数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            //开启事务处理
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                //打开块表记录
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                //加入直线
                btr.AppendEntity(line1);
                //更新数据
                tr.AddNewlyCreatedDBObject(line1, true);
                //提交
                tr.Commit();
            }
            ed.WriteMessage("生成了一条直线,起点为{0},终点为{1}", line1.StartPoint.ToString(), line1.EndPoint.ToString());
        }

        [CommandMethod("AddLineDemo")]
        public static void AddLineDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Line line1 = new Line(new Point3d(100, 100, 0), new Point3d(400, 100, 0));

            AddEntityTool.AddEntityToModelSpace(db, line1);

            List<Entity> lines = new List<Entity>();
            for (int i = 100; i < 1000; i += 100)
            {
                lines.Add(new Line(new Point3d(100 + i, 100 + i, 0), new Point3d(100 + i, 200 + i, 0)));
            }
            AddEntityTool.AddEntityToModelSpace(db, lines);
            AddEntityTool.AddLineToModelSpace(db, -200, -800, 500, 100);
            AddEntityTool.AddLineToModelSpace(db, new Point3d(20, 80, 0), 180, 45);
        }

        [CommandMethod("AddArcDemo")]
        public static void AddArcDemo()
        {
            Arc arc1 = new Arc();
            arc1.Center = new Point3d(0, 0, 0);
            arc1.StartAngle = -Math.PI / 4;
            arc1.EndAngle = Math.PI / 4;
            arc1.Radius = 80;
            Arc arc2 = new Arc(new Point3d(50, 50, 0), 20, 45, 90);
            Arc arc3 = new Arc(new Point3d(100, 100, 0), new Vector3d(0, 0, 1), 20, Math.PI / 4, Math.PI / 2);
            Database db = HostApplicationServices.WorkingDatabase;
            AddEntityTool.AddEntityToModelSpace(db, new List<Entity>() { arc1, arc2, arc3 });

            for (int i = 0; i < 90; i += 5)
            {
                Arc arc = new Arc(new Point3d(0, 0, 0), 50 + i * 10, -BaseTool.DegreeToRadian(i), BaseTool.DegreeToRadian(i));
                AddEntityTool.AddEntityToModelSpace(db, arc);
            }

        }

        [CommandMethod("AddArcDemo1")]
        public static void AddArcDemo1()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Point3d startPoint = new Point3d(100, 100, 0);
            Point3d endPoint = new Point3d(200, 200, 0);
            Point3d midPoint = new Point3d(150, 100, 0);

            AddEntityTool.AddArcToModelSpace(db, startPoint, midPoint, endPoint);
            AddEntityTool.AddArcToModelSpace(db, new Point3d(100, 0, 0), new Point3d(100, 100, 0), new Point3d(0, 100, 0));
            AddEntityTool.AddArcToModelSpace(db, new Point3d(300, 300, 0), new Point3d(500, 700, 0), 90);

        }

        [CommandMethod("AddCircleDemo")]
        public static void AddCircleDemo()
        {
            Circle c1 = new Circle();
            c1.Center = new Point3d(100, 100, 0);
            c1.Radius = 300;
            Circle c2 = new Circle(new Point3d(100, 50, 0), new Vector3d(0, 0, 1), 300);
            Database db = HostApplicationServices.WorkingDatabase;
            //AddEntityTool.AddEntityToModelSpace(db,c1);
            //AddEntityTool.AddEntityToModelSpace(db,c2);
            //AddEntityTool.AddCircleToModelSpace(db, new Point3d(0, 0, 0), 150);
            for (int i = -10000; i < 10000; i += 100)
            {
                AddEntityTool.AddCircleToModelSpace(db, new Point3d(0, 100, 0), new Point3d(100 + i, 100, 0), new Point3d(100, 0, 0));
            }

        }

        [CommandMethod("AddPolyLineDemo")]
        public static void AddPolyLineDemo()
        {
            #region
            Polyline polyline = new Polyline();
            Point2d p1 = new Point2d(100, 100);
            Point2d p2 = new Point2d(200, 100);
            Point2d p3 = new Point2d(200, 200);
            polyline.AddVertexAt(0, p1, 0, 0, 0);
            polyline.AddVertexAt(1, p2, 0, 0, 0);
            polyline.AddVertexAt(2, p3, 0, 0, 0);
            polyline.Closed = true; //自动闭合
            polyline.ConstantWidth = 10;  //设置宽度

            Database db = HostApplicationServices.WorkingDatabase;
            //AddEntityTool.AddEntityToModelSpace(db, polyline);

            //AddEntityTool.AddPolyLineToModelSpace(db, false, 0, new Point2d(0, 10), new Point2d(40, 10), new Point2d(100, 50), new Point2d(200, 80));
            List<Point2d> points = new List<Point2d>();
            for (int i = 1; i < 20; i++)
            {
                points.Add(new Point2d(i, 0.1 * i * i));
            }
            //AddEntityTool.AddPolyLineToModelSpace(db, false, 0, points);
            #endregion

            Polyline pLine1 = new Polyline();
            Polyline pLine2 = new Polyline();
            Polyline pLine3 = new Polyline();

            Point2d p4 = new Point2d(100, 100);
            Point2d p5 = new Point2d(100, 200);

            pLine1.AddVertexAt(0, p4, 1, 0, 0);
            pLine1.AddVertexAt(1, p5, 0, 0, 0);

            pLine2.AddVertexAt(0, p4, 0.5, 0, 0);
            pLine2.AddVertexAt(1, p5, 0, 0, 0);

            pLine3.AddVertexAt(0, p4, 0.2, 0, 0);
            pLine3.AddVertexAt(1, p5, 0, 0, 0);

            //AddEntityTool.AddEntityToModelSpace(db, pLine1,pLine2,pLine3);

            List<Polyline> pLines = new List<Polyline>();
            for (int i = 0; i < 10; i++)
            {
                pLines.Add(new Polyline());
            }
            foreach (Polyline pLine in pLines)
            {
                for (double j = -5; j < 5; j += 0.2)
                {
                    pLine.AddVertexAt(0, new Point2d(300 * pLines.IndexOf(pLine), 300), j, 0, 0);
                    pLine.AddVertexAt(0, new Point2d(800 * pLines.IndexOf(pLine), 300), 0, 0, 0);
                }
            }
            AddEntityTool.AddPolyLineToModelSpace(db, pLines);



        }

        [CommandMethod("AddRectDemo")]
        public static void AddRectDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Point2d point1 = new Point2d(80, 200);
            Point2d point2 = new Point2d(300, 500);

            Point2d point3 = new Point2d(100, 200);
            Point2d point4 = new Point2d(300, 150);

            //AddEntityTool.AddRectToModelSpace(db, point1, point2);
            //AddEntityTool.AddRectToModelSpace(db, point3, point4);

            AddEntityTool.AddPolygonToModelSpace(db, new Point2d(0, 0), 50, 5, 0);
            AddEntityTool.AddPolygonToModelSpace(db, new Point2d(0, 0), 100, 5, 45);
            AddEntityTool.AddPolygonToModelSpace(db, new Point2d(0, 0), 150, 5, 90);
            AddEntityTool.AddPolygonToModelSpace(db, new Point2d(0, 0), 200, 5, 120);
            double degree = 0;
            int edges = 3;
            for (int i = 50; i < 2000; i += 100)
            {
                AddEntityTool.AddPolygonToModelSpace(db, new Point2d(0, 0), i, edges, degree);
                edges += 1;
                degree += 30;
            }
        }

        [CommandMethod("FileReadDemo")]
        public static void FileReadDemo()
        {
            string filepath = @"C:\Users\yf\source\repos\CAD\CAD\out.txt";
            string[] contents = File.ReadAllLines(filepath);
            List<List<string>> lineList = new List<List<string>>();
            for (int i = 0; i < contents.Length; i++)
            {
                string[] cont = contents[i].Split(new char[] { ',' });
                List<string> subList = new List<string>();
                foreach (string str in cont)
                {
                    subList.Add(str);
                }
                lineList.Add(subList);
            }

            List<Point2d> points = new List<Point2d>();
            foreach (List<string> ct in lineList)
            {
                double x, y;
                bool bx = double.TryParse(ct[0], out x);
                bool by = double.TryParse(ct[1], out y);
                if (bx || by)
                {
                    points.Add(new Point2d(x, y));
                }
                else
                {
                    Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                    ed.WriteMessage("外部文件有误,绘图失败!");
                    return;
                }
                Database db = HostApplicationServices.WorkingDatabase;

                AddEntityTool.AddPolyLineToModelSpace(db, false, 0, points);
            }
        }

        [CommandMethod("AddEllipseDemo")]
        public static void AddEllipseDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Ellipse el1 = new Ellipse();
            Ellipse el2 = new Ellipse(new Point3d(100, 100, 0), Vector3d.ZAxis, new Vector3d(100, 0, 0), 0.4, 0, Math.PI);
            //AddEntityTool.AddEntityToModelSpace(db, el2);
            //AddEntityTool.AddEllipseToModelSpace(db, new Point3d(100, 200, 0), 500, 200, 0, 250, 45);
            //AddEntityTool.AddEllipseToModelSpace(db, new Point3d(0, 0, 0), 100, 60, 0, 360, 20);
            //AddEntityTool.AddEllipseToModelSpace(db, new Point3d(100, 200, 0), 500, 200);
            //AddEntityTool.AddEllipseToModelSpace(db, new Point3d(100, 0, 0), new Point3d(0, 100, 0), 20);
            //AddEntityTool.AddEllipseToModelSpace(db, new Point3d(100, 400, 0), new Point3d(0, 100, 0), 20,90,270);
            AddEntityTool.AddRectToModelSpace(db, new Point2d(200, 200), new Point2d(1500, 800));
            AddEntityTool.AddEllipseToModelSpace(db, new Point3d(200, 200, 0), new Point3d(1500, 800, 0));

            AddEntityTool.AddRectToModelSpace(db, new Point2d(200, 200), new Point2d(600, 800));
            AddEntityTool.AddEllipseToModelSpace(db, new Point3d(200, 200, 0), new Point3d(600, 800, 0));
        }

        [CommandMethod("AddHatchDemo")]
        public static void AddHatchDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            ObjectId cId = AddEntityTool.AddCircleToModelSpace(db, new Point3d(100, 100, 0), 500);
            HatchTool.HatchEntity(db, HatchTool.HatchPattern.GRASS, cId, Color.FromColorIndex(ColorMethod.ByColor, 9), 1, 60, 6);

            ObjectId c1 = AddEntityTool.AddCircleToModelSpace(db, new Point3d(500, 500, 0), 200);
            ObjectId c2 = AddEntityTool.AddCircleToModelSpace(db, new Point3d(500, 500, 0), 100);
            List<HatchLoopTypes> loopTypes = new List<HatchLoopTypes>();
            List<ObjectId> entsId = new List<ObjectId>();

            loopTypes.Add(HatchLoopTypes.Outermost);
            loopTypes.Add(HatchLoopTypes.Outermost);
            entsId.Add(c1);
            entsId.Add(c2);
            HatchTool.HatchEntity(db, HatchTool.HatchPattern.NET3, loopTypes, entsId, Color.FromColorIndex(ColorMethod.ByColor, 19), 5, 30, 3);

            ObjectId c3 = AddEntityTool.AddCircleToModelSpace(db, new Point3d(1000, 500, 0), 200);
            ObjectId c4 = AddEntityTool.AddCircleToModelSpace(db, new Point3d(1000, 500, 0), 100);
            ObjectId c5 = AddEntityTool.AddCircleToModelSpace(db, new Point3d(1000, 500, 0), 50);
            loopTypes.Add(HatchLoopTypes.Outermost);
            List<ObjectId> entsId1 = new List<ObjectId>();
            entsId1.Add(c3);
            entsId1.Add(c4);
            entsId1.Add(c5);

            HatchTool.HatchEntity(db, HatchTool.HatchPattern.FLEX, loopTypes, entsId1, Color.FromColorIndex(ColorMethod.ByColor, 10), 3, 30, 3);




        }

        [CommandMethod("AddHatchDemo1")]
        public static void AddHatchDemo1()
        {

            Database db = HostApplicationServices.WorkingDatabase;
            ObjectId c1 = AddEntityTool.AddCircleToModelSpace(db, new Point3d(100, 100, 0), 500);
            ObjectId rect1 = AddEntityTool.AddRectToModelSpace(db, new Point2d(800, 900), new Point2d(1500, 1700));
            HatchTool.HatchGradient(db, HatchTool.HatchGradientPattern.GR_LINEAR, 5, 10, c1, 45);
            HatchTool.HatchGradient(db, HatchTool.HatchGradientPattern.GR_INVCUR, 7, 20, rect1, 30);
        }

        [CommandMethod("EditDemo")]
        public static void EditDemo()
        {
            //更改图形颜色
            Database db = HostApplicationServices.WorkingDatabase;
            Circle c1 = new Circle(new Point3d(100, 100, 0), new Vector3d(0, 0, 1), 100);
            Circle c2 = new Circle(new Point3d(300, 100, 0), new Vector3d(0, 0, 1), 100);
            c1.ColorIndex = 1;
            c2.Color = Color.FromRgb(100, 203, 50); //加到图形数据库后属性不能直接改
            EditEntityTool.ChangeEntityColor(db, c1, 30);
            ObjectId c1Id = AddEntityTool.AddEntityToModelSpace(db, c1);
            ObjectId c2Id = AddEntityTool.AddEntityToModelSpace(db, c2);

            #region
            //加到图形数据库的图形用事务来更改属性
            //using (Transaction tr = db.TransactionManager.StartTransaction())
            //{
            //    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            //    BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            //    Entity ent1 = c1Id.GetObject(OpenMode.ForWrite) as Entity;
            //    Entity ent2 = c2Id.GetObject(OpenMode.ForWrite) as Entity;
            //    ent1.ColorIndex = 6;
            //    ent2.Color = Color.FromRgb(20, 200, 100);
            //    tr.Commit();
            //}
            #endregion

            //封装好的函数

            EditEntityTool.ChangeEntityColor(db, c2Id, 80);


        }

        [CommandMethod("MoveDemo")]
        public static void MoveDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Circle c1 = new Circle(new Point3d(100, 100, 0), new Vector3d(0, 0, 1), 50);
            Circle c2 = new Circle(new Point3d(100, 100, 0), new Vector3d(0, 0, 1), 50);
            Circle c3 = new Circle(new Point3d(100, 100, 0), new Vector3d(0, 0, 1), 50);

            Point3d p1 = new Point3d(100, 100, 0);
            Point3d p2 = new Point3d(200, 300, 0);
            //c2.Center = new Point3d(c2.Center.X+p2.X-p1.X, c2.Center.Y+p2.Y-p1.Y, 0);
            AddEntityTool.AddEntityToModelSpace(db, c1);
            AddEntityTool.AddEntityToModelSpace(db, c3);
            AddEntityTool.AddEntityToModelSpace(db, c2);

            EditEntityTool.MoveEntity(db, c3, p2, p1);
            EditEntityTool.MoveEntity(db, c2, p1, p2);


        }

        [CommandMethod("CopyDemo")]
        public static void CopyDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Circle c1 = new Circle(new Point3d(100, 100, 0), Vector3d.ZAxis, 100);
            EditEntityTool.CopyEntity(db, c1, new Point3d(100, 100, 0), new Point3d(100, 200, 0));
            AddEntityTool.AddEntityToModelSpace(db, c1);
            EditEntityTool.CopyEntity(db, c1, new Point3d(0, 0, 0), new Point3d(-100, 0, 0));
        }

        [CommandMethod("RotateDemo")]
        public static void RotateDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Line l1 = new Line(new Point3d(100, 100, 0), new Point3d(300, 100, 0));
            Line l2 = new Line(new Point3d(100, 100, 0), new Point3d(300, 100, 0));
            Line l3 = new Line(new Point3d(100, 100, 0), new Point3d(300, 100, 0));

            AddEntityTool.AddEntityToModelSpace(db, l1);
            EditEntityTool.RotateEntity(db, l1, new Point3d(100, 100, 0), 30);
            EditEntityTool.RotateEntity(db, l2, new Point3d(0, 0, 0), 60);
            EditEntityTool.RotateEntity(db, l3, new Point3d(200, 500, 0), 90);
            AddEntityTool.AddEntityToModelSpace(db, l2, l3);

        }

        [CommandMethod("MirrorDemo")]
        public static void MirrorDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Circle c1 = new Circle(new Point3d(100, 100, 0), Vector3d.ZAxis, 50);
            ObjectId c1Id = AddEntityTool.AddEntityToModelSpace(db, c1);

            EditEntityTool.MirrorEntity(db, c1Id, new Point3d(200, 100, 0), new Point3d(200, 300, 0), true);


            Circle c2 = new Circle(new Point3d(300, 300, 0), Vector3d.ZAxis, 50);
            EditEntityTool.MirrorEntity(db, c2, new Point3d(200, 100, 0), new Point3d(200, 300, 0));


        }

        [CommandMethod("ScaleDemo")]
        public static void ScaleDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Circle c1 = new Circle(new Point3d(100, 100, 0), Vector3d.ZAxis, 50);
            AddEntityTool.AddEntityToModelSpace(db, c1);

            Circle c2 = new Circle(new Point3d(200, 100, 0), Vector3d.ZAxis, 50);
            EditEntityTool.ScaleEntity(db, c2, new Point3d(200, 100, 0), 0.5);
            AddEntityTool.AddEntityToModelSpace(db, c2);

            Circle c3 = new Circle(new Point3d(300, 100, 0), Vector3d.ZAxis, 50);
            AddEntityTool.AddEntityToModelSpace(db, c3);
            EditEntityTool.ScaleEntity(db, c3, new Point3d(0, 0, 0), 2);

        }

        [CommandMethod("ArrayRectDemo")]
        public static void ArrayRectDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            //ObjectId c1Id = AddEntityTool.AddCircleToModelSpace(db, new Point3d(0, 0, 0), 20);
            //EditEntityTool.ArrayRectEntity(db, c1Id, 10, 20, 30, 50); // 向反方向阵列可以将间距设为负数
            Circle c2 = new Circle(new Point3d(100, 100, 0), Vector3d.ZAxis, 10);
            List<Entity> ents = EditEntityTool.ArrayRectEntity(db, c2, 3, 5, 20, 20); //阵列
            //旋转
            foreach (Entity ent in ents)
            {
                EditEntityTool.RotateEntity(db, ent, new Point3d(0, 0, 0), 45);
            }
        }

        [CommandMethod("RingArrayDemo")]
        public static void RingArrayDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            ObjectId lineId1 = AddEntityTool.AddLineToModelSpace(db, new Point3d(100, 100, 0), new Point3d(120, 100, 0));
            EditEntityTool.ArrayRingEntity(db, lineId1, 6, 360, new Point3d(100, 100, 0));

            ObjectId lineId2 = AddEntityTool.AddLineToModelSpace(db, new Point3d(400, 100, 0), new Point3d(600, 100, 0));
            EditEntityTool.ArrayRingEntity(db, lineId2, 10, 360, new Point3d(1000, 100, 0));

            ObjectId lineId3 = AddEntityTool.AddLineToModelSpace(db, new Point3d(500, 100, 0), new Point3d(1120, 100, 0));
            EditEntityTool.ArrayRingEntity(db, lineId3, 6, -120, new Point3d(2000, 100, 0));

        }
        #endregion

        [CommandMethod("PromptDemo")]
        public static void PromptDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //PromptPointResult ppr =  ed.GetPoint("\n请选择一个点:");
            //if (ppr.Status == PromptStatus.OK)
            //{
            //    PromptPointResult ppr1 = ed.GetPoint("\n请选择第二个点:");
            //    if(ppr1.Status == PromptStatus.OK)
            //    {
            //        AddEntityTool.AddLineToModelSpace(db, ppr.Value, ppr1.Value);
            //    }
            //}
            PromptPointOptions ppo = new PromptPointOptions("请指定第一个点:");
            PromptPointResult ppr = GetPoint(ppo);
            ppo.AllowNone = true;
            Point3d p1 = new Point3d(0, 0, 0);
            Point3d p2 = new Point3d();
            if (ppr.Status == PromptStatus.Cancel) return;
            if (ppr.Status == PromptStatus.OK) { p1 = ppr.Value; }

            ppo.Message = "请指定第二个点:";
            ppo.BasePoint = p1;
            ppo.UseBasePoint = true;
            ppr = GetPoint(ppo);
            if (ppr.Status == PromptStatus.Cancel) return;
            if (ppr.Status == PromptStatus.None) return;
            if (ppr.Status == PromptStatus.OK) p2 = ppr.Value;
            db.AddLineToModelSpace(p1, p2);

        }

        public static PromptPointResult GetPoint(PromptPointOptions ppo)
        {
            ppo.AllowNone = true;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptPointResult ppr = ed.GetPoint(ppo);
            return ppr;
        }

        [CommandMethod("FlagLine")]
        public static void FlagLine()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //声明一个直线的集合对象
            List<ObjectId> lineList = new List<ObjectId>();

            Point3d pointStart = new Point3d(100, 100, 0);
            Point3d pointPre = new Point3d(100, 100, 0);
            PromptPointResult ppr = ed.GetPoint2("\n指定第一个点:");

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
                    ppr = ed.GetPoint("\n指定下一点或[闭合(C)/放弃(U)]:", pointPre, new string[] { "C", "U" });
                }
                else
                {
                    ppr = ed.GetPoint("\n指定下一个点或[放弃(U)]", pointPre, new string[] { "U" });
                }


                Point3d pointNext;
                if (ppr.Status == PromptStatus.Cancel) return;
                if (ppr.Status == PromptStatus.None) return;
                if (ppr.Status == PromptStatus.OK)
                {
                    pointNext = ppr.Value;
                    lineList.Add(db.AddLineToModelSpace(pointPre, pointNext));
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
                                ppr = ed.GetPoint2("\n指定第一个点:");
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
                                pointPre = CAD.GetLineStartPoint(lineId);
                                lineList.RemoveAt(count - 1);
                                EditEntityTool.EraseEntity(db, lineId);

                            }
                            break;
                        case "C":
                            lineList.Add(db.AddLineToModelSpace(pointPre, pointStart));
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

        [CommandMethod("AddCircleDemo2")]
        public static void AddCircleDemo2()
        {

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Point3d center1 = new Point3d();
            //double radius = 0;
            PromptPointResult ppr = ed.GetPoint("\n请指定圆心:");
            if (ppr.Status == PromptStatus.OK)
            {
                center1 = ppr.Value;
            }
            CircleJig jCircle = new CircleJig(center1);
            PromptPointResult pr = ed.Drag(jCircle) as PromptPointResult;
            //法一
            //if (pr.Status == PromptStatus.OK)
            //{
            //    Point3d point = pr.Value;
            //    db.AddCircleToModelSpace(center1, point.GetDistanceBetweenTwoPoint(center1));
            //}

            //法二
            if (pr.Status == PromptStatus.OK)
            {
                db.AddEntityToModelSpace(jCircle.GetEntity());
            }




        }

        /// <summary>
        /// 获取直线的起点坐标
        /// </summary>
        /// <param name="lineId">直线的ObjectId</param>
        /// <returns>起点</returns>
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

        [CommandMethod("FangLine")]
        public static void FangLine()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //声明一个直线的集合对象
            List<ObjectId> lineList = new List<ObjectId>();

            Point3d pointStart = new Point3d(100, 100, 0);
            Point3d pointPre = new Point3d(100, 100, 0);
            PromptPointResult ppr = ed.GetPoint2("\n指定第一个点:");

            if (ppr.Status == PromptStatus.Cancel) return;
            if (ppr.Status == PromptStatus.None) pointPre = pointStart;
            if (ppr.Status == PromptStatus.OK)
            {
                pointStart = ppr.Value;
                pointPre = pointStart;
            }

            PromptResult pr;
            LineJig lineJig;
            //判断循环是否继续
            bool isC = true;
            while (isC)
            {
                if (lineList.Count > 1)
                {
                    //ppr = ed.GetPoint("\n指定下一点或[闭合(C)/放弃(U)]:", pointPre, new string[] { "C", "U" });
                    lineJig = new LineJig(pointPre, "\n指定下一点或[闭合(C)/放弃(U)]:", new string[] { "C", "U" });
                    pr = ed.Drag(lineJig);

                }
                else
                {
                    //ppr = ed.GetPoint("\n指定下一个点或[放弃(U)]", pointPre, new string[] { "U" });
                    lineJig = new LineJig(pointPre, "\n指定下一点或放弃(U)]:", new string[] { "U" });
                    pr = ed.Drag(lineJig);
                }


                //Point3d pointNext;
                if (pr.Status == PromptStatus.Cancel) return;
                if (pr.Status == PromptStatus.None) return;
                if (pr.Status == PromptStatus.OK)
                {
                    //pointNext = ppr.Value;
                    //lineList.Add(db.AddLineToModelSpace(pointPre, pointNext));
                    //pointPre = pointNext;
                    Line line = lineJig.GetEntity() as Line;
                    pointPre = line.EndPoint;
                    lineList.Add(db.AddEntityToModelSpace(line));

                }
                if (pr.Status == PromptStatus.Keyword)
                {
                    switch (pr.StringResult)
                    {
                        case "U":
                            if (lineList.Count == 0)
                            {
                                pointStart = new Point3d(100, 100, 0);
                                pointPre = new Point3d(100, 100, 0);
                                ppr = ed.GetPoint2("\n指定第一个点:");
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
                                pointPre = CAD.GetLineStartPoint(lineId);
                                lineList.RemoveAt(count - 1);
                                EditEntityTool.EraseEntity(db, lineId);

                            }
                            break;
                        case "C":
                            lineList.Add(db.AddLineToModelSpace(pointPre, pointStart));
                            isC = false;
                            break;
                        case " ":
                            isC = false;
                            break;

                    }
                }
            }

        }

        [CommandMethod("SelectDemo")]
        public static void SelectDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //选择集一
            //ed.SelectAll(); //得到所有图形
            TypedValue[] values = new TypedValue[] {
                new TypedValue((int)DxfCode.Start,"CIRCLE")
            };
            SelectionFilter filter = new SelectionFilter(values);
            PromptSelectionResult psr = ed.GetSelection(filter); //获得选中的图形
            if (psr.Status == PromptStatus.OK)
            {
                SelectionSet sSet = psr.Value;
                ChangeColor(sSet);
            }




        }

        [CommandMethod("MoveDemo1")]
        public static void MoveDemo1()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //会接收命令执行前用记选择的图形对象
            PromptSelectionResult psr = ed.SelectImplied();
            //如果执行命令前用记没有选择图形对象，就提示选择
            if (psr.Status != PromptStatus.OK)
            {
                psr = ed.GetSelection();
            }
            if (psr.Status != PromptStatus.OK) return;

            //获取用户指定的基点
            Point3d pointBase = new Point3d(0, 0, 0);
            PromptPointOptions ppo = new PromptPointOptions("\n指定基点或[位移(D)] <位移>:");
            ppo.AllowNone = true;
            PromptPointResult ppr = ed.GetPoint(ppo);
            //判断用户指定基点的输入
            if (ppr.Status == PromptStatus.Cancel) return;
            if (ppr.Status == PromptStatus.OK) pointBase = ppr.Value;

            //获取选择图形的图形对象；
            List<Entity> entList = new List<Entity>();
            ObjectId[] ids = psr.Value.GetObjectIds();
            entList = GetEntity(ids);
            //复制图形
            Matrix3d mt = Matrix3d.Displacement(new Vector3d(0, 0, 0));
            List<Entity> entListCopy = CopyEntity(entList, mt);
            List<Entity> entListCopy2 = CopyEntity(entList, mt);

            //改变原图形颜色
            LowColorEntity(entList, 112);

            MoveJig moveJig = new MoveJig(entListCopy2, pointBase);
            PromptResult pr = ed.Drag(moveJig);
            if (pr.Status == PromptStatus.OK)
            {
                List<Entity> ents = moveJig.GetEntityList();
                db.AddEntityToModelSpace(ents);
                DeleteEntitys(ents.ToArray());
            }
            if (pr.Status == PromptStatus.Cancel)
            {
                db.AddEntityToModelSpace(entListCopy2);
                DeleteEntitys(entList.ToArray());
            }
            if (pr.Status == PromptStatus.Keyword && pr.StringResult == " ")
            {
                Vector3d vector = Point3d.Origin.GetVectorTo(pointBase);
                mt = Matrix3d.Displacement(vector);
                for (int i = 0; i < entListCopy2.Count; i++)
                {
                    EditEntityTool.MoveEntity(db, entListCopy2[i], Point3d.Origin, pointBase);
                }
                db.AddEntityToModelSpace(entListCopy);
                DeleteEntitys(entList.ToArray());
            }

        }

        /// <summary>
        /// 单行文字
        /// </summary>
        [CommandMethod("TextDmeo")]
        public static void TextDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;

            Line[] lines = new Line[10];
            Point3d[] pt1s = new Point3d[10];
            Point3d[] pt2s = new Point3d[10];
            for (int i = 0; i < lines.Length; i++)
            {
                pt1s[i] = new Point3d(50, 50 + 20 * i, 0);
                pt2s[i] = new Point3d(150, 50 + 20 * i, 0);
                lines[i] = new Line(pt1s[i], pt2s[i]);
            }
            db.AddEntityToModelSpace(lines);

            Random rd = new Random();
            List<DBText> texts = new List<DBText>(10);
            for (int i = 0; i < 10; i++)
            {
                texts.Add(new DBText());
                texts[i].TextString = "这是第" + i + "行";
                texts[i].Position = pt1s[i];
                texts[i].ColorIndex = rd.Next(0, 255);
                texts[i].Height = (i + 1) * 5;
            }
            texts[0].IsMirroredInX = true;
            texts[1].IsMirroredInY = true;
            //texts[2].AlignmentPoint = pt1s[2];  // 对齐点
            texts[3].Thickness = 10;
            texts[4].Rotation = Math.PI * 0.25;
            texts[5].HorizontalMode = TextHorizontalMode.TextCenter;   // 水平对齐方式
            texts[5].AlignmentPoint = texts[5].Position;             //对齐点
            texts[6].HorizontalMode = TextHorizontalMode.TextFit;
            texts[7].Oblique = 100;  //倾斜

            db.AddEntityToModelSpace(texts.ToArray());





        }

        [CommandMethod("MTextsDemo")]
        public static void MTextDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            MText mtext = new MText();
            mtext.Location = new Point3d(100, 100, 0);
            mtext.Contents = "hello, this is my \nfirst mtext demo.";
            mtext.Width = 100;
            mtext.Height = 300;
            mtext.TextHeight = 80;
            mtext.Rotation = Math.PI * 0.25;
            mtext.LineSpacingStyle = LineSpacingStyle.Exactly;

            db.AddEntityToModelSpace(mtext);



        }

        [CommandMethod("CaiYang")]
        public static void CaiYang()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptEntityResult perBase = ed.GetEntity("\n 请选择基准线:");
            if (perBase.Status != PromptStatus.OK) return;
            ObjectId baseEntityId = perBase.ObjectId;
            PromptEntityResult perCurve = ed.GetEntity("\n 请译择采样线:");
            if (perCurve.Status != PromptStatus.OK) return;
            //获取基准线和采样线的图形实体对象
            Entity baseEntity = GetEntity(db, perBase.ObjectId);
            Entity curveEntity = GetEntity(db, perCurve.ObjectId);

            if (baseEntity is Line)
            {
                Line baseLine = (Line)baseEntity;
                List<Point3d> divPoints1 = GetBaseLineDivPoints(baseLine,160.0); //定距分点
                //List<Point3d> divPoints2 = GetBaseLineDivPoints(baseLine, 100); //定数分点
                //在基准线上画等分点
                //List<DBPoint> dbPoints = new List<DBPoint>(); 
                //for(int i = 0; i < divPoints2.Count; i++)
                //{
                //    dbPoints.Add(new DBPoint(divPoints2[i]));
                //}
                //AddEntity(db, dbPoints.ToArray());

                List<Line> divLines = GetDivLines(divPoints1.ToArray(), 90, 500);
                AddEntity(db, divLines.ToArray());



            }
            else
            {
                ed.WriteMessage("\n基准线必须为直线:");
            }
            

        }

        [CommandMethod("RotatedDimDemo")]
        public static void RotatedDimDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Line line = new Line(new Point3d(100, 100, 0), new Point3d(200, 500, 0));
            RotatedDimension rotatedDim = new RotatedDimension();

            Point3d p1 = new Point3d(100, 100, 0);
            Point3d p2 = new Point3d(200, 500, 0);
            

            rotatedDim.XLine1Point = new Point3d(100, 100, 0);  //第一个标注点
            rotatedDim.XLine2Point = new Point3d(200,500,0);    // 第二个标注点
            rotatedDim.DimLinePoint = new Point3d(300, 150, 0);  //标注横线的位置
            rotatedDim.TextRotation = Math.PI /3;  //标注数字旋转角
            rotatedDim.DimensionText = "<>米";  //加单位
            rotatedDim.Rotation = p1.GetVectorTo(p2).GetAngleTo(Vector3d.XAxis);  //标注线的角度
            rotatedDim.TextLineSpacingFactor = 50;
            rotatedDim.AlternatePrefix = "Prefix";
            rotatedDim.AlternateSuffix = "Suffix";
            rotatedDim.Dimasz = 20; //箭头大小

            db.AddEntityToModelSpace(line);
            db.AddEntityToModelSpace(rotatedDim);
        }

        [CommandMethod("DimDemo")]
        public static void DimDemo()
        {
            //对齐标注
            Database db = HostApplicationServices.WorkingDatabase;

            AlignedDimension aDim = new AlignedDimension();
            Point3d p1 = new Point3d(10, 10, 0);
            Point3d p2 = new Point3d(200, 150, 0);
            aDim.XLine1Point = p1;
            aDim.XLine2Point = p2;
            aDim.DimLinePoint = new Point3d(10, 20, 0);
            aDim.Dimasz = 10;
            db.AddEntityToModelSpace(aDim);

            //角度标注
            LineAngularDimension2 lDim = new LineAngularDimension2();
            Point3d p3 = new Point3d(-50, 200, 0);
            lDim.XLine1Start = p1;
            lDim.XLine1End = p2;
            lDim.XLine2Start = p1;
            lDim.XLine2End = p3;
            lDim.ArcPoint = new Point3d(50, 50, 0);   //这个点会在标注弧上
            Line line1 = new Line(p1, p2);
            Line line2 = new Line(p1, p3);
            db.AddEntityToModelSpace(line1, line2);
            db.AddEntityToModelSpace(lDim);

            //三点标注
            Point3AngularDimension p3Dim = new Point3AngularDimension();
            p3Dim.CenterPoint = new Point3d(10, 10, 0);
            p3Dim.XLine1Point = new Point3d(20, 12, 0);
            p3Dim.XLine2Point = new Point3d(20, 20, 0);
            p3Dim.ArcPoint = new Point3d(25, 15, 0);
            db.AddEntityToModelSpace(p3Dim);
        }

        [CommandMethod("DimDemo1")]
        public static void DimDemo1()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            //弧长标注
            //ArcDimension arcDim = new ArcDimension(new Point3d(10,10,0),new Point3d(20,10,0),new Point3d(10,20,0),new Point3d(10,20,0),"<>",db.Dimstyle);
            Arc arc = new Arc();
            arc.Center = new Point3d(10, 10, 0);
            arc.Radius = 50;
            arc.StartAngle = 0;
            arc.EndAngle = Math.PI * 0.25;

            ArcDimension arcDim = new ArcDimension(arc.Center, arc.StartPoint, arc.EndPoint, new Point3d(arc.EndPoint.X + 5, arc.EndPoint.Y + 5, arc.EndPoint.Z), "<>", db.Dimstyle);
            db.AddEntityToModelSpace(arcDim);
            db.AddArcToModelSpace(arc);

            //半径标注
            RadialDimension rDim = new RadialDimension();
            rDim.Center = new Point3d(100, 100, 0);
            rDim.ChordPoint = new Point3d(500, 300, 0);
            rDim.LeaderLength = 10; //引出的线的长度
            rDim.TextRotation = Math.PI * 0.25;
            rDim.HorizontalRotation = 0;
           
            db.AddEntityToModelSpace(rDim);

            //直径标注
            DiametricDimension dDim = new DiametricDimension();
            dDim.ChordPoint = new Point3d(10, 10, 0);       //直径上的一个点
            dDim.FarChordPoint = new Point3d(50, 20, 0);    //直径上的另一个点
            dDim.LeaderLength = 30;
            dDim.TextRotation = 0;
            dDim.HorizontalRotation = 0;
            db.AddEntityToModelSpace(dDim);

        }

        [CommandMethod("LayerDemo")]
        public static void LayerDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            //AddLayerResult alr =  db.AddLayer("111");
            //AddLayerResult alr1 = db.AddLayer("222");
            //db.ChangeLayerColor(alr1.LayerName, 1);
            //db.ChangeLayerLockStatus(alr.LayerName, true);
            //db.ChangeLayerLineWeight(alr.LayerName, LineWeight.LineWeight040);
            //db.SetCurrentLayer(alr1.LayerName);
            //db.GetAllLayersName();
            //db.DeleteLayer(alr.LayerName);

            db.DeleteLayer("图层1",true);
            db.DeleteNotUsedLayer();
        }

        [CommandMethod("TextStyleDemo")]
        public static void TextStyleDemo()
        {

        }

        [CommandMethod("DimSytleDemo")]
        public static void DimStyleDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            ObjectId dimStyleId = db.AddDimStyle("我的注释样式");

        }

        //动态画图，但是无效
        [CommandMethod("DD", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public static void DD()
        {
            Database db = HostApplicationServices.WorkingDatabase;//当前的数据库
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            int aa = 0;
            for (int i = 0; i < 50; i++)
            {
                //新建圆
                AddEntity(db, new Circle(new Point3d(aa++, 0, 0),Vector3d.ZAxis, 0.5));
                //刷新内容
                Application.UpdateScreen();
                ed.WriteMessage(aa++.ToString() + "\n");//这两种不同感觉 
                //ed.WriteMessage(aa++.ToString()+"\r");//这两种不同感觉
                //高版本要加这句令命令栏立即执行
                System.Windows.Forms.Application.DoEvents();

                //阻塞
                //Thread.Sleep(100);
                for(int j = 100; j > 0; j--) { }
            }
            
        }

        [CommandMethod("BlockDemo")]
        public static void BlockDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            MyBlockTableRecord.Block1Id =  db.AddBlockTableRecord(MyBlockTableRecord.Block1Name, MyBlockTableRecord.Block1Ents);

        }

        [CommandMethod("InsertDemo")]
        public static void InsertDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            db.InsertBlockReference(MyBlockTableRecord.Block1Id, new Point3d(10,10,0));
            db.InsertBlockReference(MyBlockTableRecord.Block1Id, new Point3d(40, 10, 0),Math.PI*0.25,new Scale3d(2));
            db.InsertBlockReference(MyBlockTableRecord.Block1Id, new Point3d(80, 10, 0), Math.PI * 0.25, new Scale3d(2.0,1.5,0));
        }

        [CommandMethod("BrickWall")]
        public static void BrickWall()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            double width = 30;
            double height = 20;
            double xSpace = 15;
            double ySpace = 20;
            for(int i = 0; i < 50; i++)
            {
                for(int j = 0; j < 80; j++)
                {
                    db.AddRectToModelSpace(new Point2d(0+(width+xSpace)*j, 0+(height+ySpace)*i), width, height);
                    db.AddCircleToModelSpace(new Point3d(0 + (width + xSpace) * j, 0 + (height + ySpace) * i, 0), Math.Min(width,height)/2);
                }
            }
        }

        [CommandMethod("TableDemo")]
        public static void TableDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Color color = Color.FromColorIndex(ColorMethod.ByAci, 5);
            Color color1 = Color.FromColorIndex(ColorMethod.ByAci, 10);
            Color color2 = Color.FromRgb(10, 20, 100);

            Table table = new Table();
            table.SetSize(10, 5);
            table.SetRowHeight(20);
            table.SetColumnWidth(50);
            table.Columns[3].Name = "hello";
            table.Position = new Point3d(0, 0, 0);
            table.Rotation = Math.PI * 0.25;
            table.Cells[0, 0].TextString = "材料表";
            table.Columns[0].Width = 10;
            table.Rows[3].BackgroundColor = color;
            table.Columns[4].BackgroundColor = color1;
            table.Cells[2, 3].TextString = "[2,3]";
            db.AddEntityToModelSpace(table);

            Table table1 = new Table();
            table1.SetSize(10, 5);
            table1.SetRowHeight(20);
            table1.SetColumnWidth(50);
            table1.Position = new Point3d(300, 0, 0);
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    table1.Cells[i, j].TextString = string.Format("[{0},{1}]", i, j);
                }
            }

            db.AddEntityToModelSpace(table1);



            //using(Transaction tr = db.TransactionManager.StartTransaction())
            //{
            //    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            //    BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            //    btr.AppendEntity(table);
            //    tr.AddNewlyCreatedDBObject(table, true);
            //    tr.Commit();
            //}
        }

        [CommandMethod("SaveToTextDemo")]
        public static void SaveToTextDemo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Wnd.SaveFileDialog saveDlg = new Wnd.SaveFileDialog();
            saveDlg.Title = "保存图形数据";
            saveDlg.Filter = "文本文件(*.txt)|*.txt";
            saveDlg.InitialDirectory = Path.GetDirectoryName(db.Filename);
            saveDlg.FileName = Path.GetFileNameWithoutExtension(db.Filename);
            Wnd.DialogResult saveDlgRes =  saveDlg.ShowDialog();
            if(saveDlgRes == Wnd.DialogResult.OK)
            {
                string[] contents = new string[] { "111", "222" };
                File.WriteAllLines(saveDlg.FileName, contents);
            }
            
        }

        [CommandMethod("OpenFileDemo")]
        public static void OpenFileDemo()
        {
            Wnd.OpenFileDialog openDlg = new Wnd.OpenFileDialog();
            openDlg.Title = "打开数据文件";
            openDlg.Filter = "文本文件(*.txt)|*.txt";
            Wnd.DialogResult openRes =  openDlg.ShowDialog();
            if (openRes == Wnd.DialogResult.OK)
            {
                string[] contents = File.ReadAllLines(openDlg.FileName);

            }
        }


        static bool flag = false;
        [CommandMethod("Anima", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public static void Anima()
        {
            flag = !flag;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            int num = 0;
            for(int i = 0; i < 25; i++)
            {
                for(int j = 0; j < 25; j++)
                {
                    Circle circle = new Circle(new Point3d(i, j, 0), Vector3d.ZAxis, 0.5);
                    db.AddEntityToModelSpace(circle);
                    num++;
                    string strNum = num.ToString();
                    if (flag)
                        strNum += "\n";
                    else
                        strNum += "\r";
                    //ed.WriteMessage(strNum);

                    UpdateTool.UpdateScreenEx(circle);
                    //System.Threading.Thread.Sleep(1);
                }
            }
            
        }

        [CommandMethod("Tetris")]
        public static void Tetris()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            double lineLength = 10;
            Point3d center = new Point3d(0, 0, 0);
            double factor = 0.6;

            for(int i = 0; i < 15; i++)
            {
                for(int j = 0; j < 25; j++)
                {
                    center = new Point3d(i * lineLength,j * lineLength, 0);
                    ObjectId innerRecId = db.AddRectToModelSpace(new Point2d(center.X - (factor / 2) * lineLength, center.Y - (factor / 2) * lineLength), new Point2d(center.X + (factor / 2) * lineLength, center.Y + (factor / 2) * lineLength));
                    ObjectId outerRecId = db.AddRectToModelSpace(new Point2d(center.X - lineLength / 2, center.Y - lineLength / 2), new Point2d(center.X + lineLength / 2, center.Y + lineLength / 2));
                    ObjectId line1 = db.AddLineToModelSpace(new Point3d(center.X - lineLength / 2, center.Y - lineLength / 2, 0), new Point3d(center.X - (factor / 2) * lineLength, center.Y - (factor / 2) * lineLength, 0));
                    ObjectId line2 = db.AddLineToModelSpace(new Point3d(center.X - lineLength / 2, center.Y + lineLength / 2, 0), new Point3d(center.X - (factor / 2) * lineLength, center.Y + (factor / 2) * lineLength, 0));
                    ObjectId line3 = db.AddLineToModelSpace(new Point3d(center.X + (factor / 2) * lineLength, center.Y + (factor / 2) * lineLength, 0), new Point3d(center.X + lineLength / 2, center.Y + lineLength / 2, 0));
                    ObjectId line4 = db.AddLineToModelSpace(new Point3d(center.X + (factor / 2) * lineLength, center.Y - (factor / 2) * lineLength, 0), new Point3d(center.X + lineLength / 2, center.Y - lineLength / 2, 0));
                    ObjectId hatch = HatchTool.HatchGradient(db, HatchTool.HatchGradientPattern.GR_LINEAR, 100, 100, innerRecId, 45);
                    UpdateTool.UpdateScreenEx(db, innerRecId);
                    UpdateTool.UpdateScreenEx(db, outerRecId);
                    UpdateTool.UpdateScreenEx(db, line1);
                    UpdateTool.UpdateScreenEx(db, line2);
                    UpdateTool.UpdateScreenEx(db, line3);
                    UpdateTool.UpdateScreenEx(db, line4);
                    UpdateTool.UpdateScreenEx(db, hatch);
                    Thread.Sleep(100);

                }
            }
        }



        public static ObjectId AddEntity(Database db, Entity ent)
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


        /// <summary>
        /// 获取等分线对象
        /// </summary>
        /// <param name="points">等分线起点</param>
        /// <param name="angle">与x轴正方向的夹角</param>
        /// <param name="length">长度</param>
        /// <returns>等分线对象列表</returns>
        public static List<Line> GetDivLines(Point3d[] points,double angle,double length)
        {
            List<Line> lines = new List<Line>();
            for(int i = 0; i < points.Length; i++)
            {
                lines.Add(new Line(points[i], PolarPoint(points[i], length, angle)));
            }
            return lines;
        }

        /// <summary>
        /// 添加图形对象到图形数据库
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="ents">图形对象数组</param>
        /// <returns>图形对象的ObjectId列表</returns>
        public static List<ObjectId> AddEntity(Database db,Entity[] ents)
        {
            List<ObjectId> entIds = new List<ObjectId>();
            using(Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                for(int i=0;i<ents.Length;i++)
                {
                    entIds.Add(btr.AppendEntity(ents[i]));
                    tr.AddNewlyCreatedDBObject(ents[i], true);
                }
                
                tr.Commit();
            }
            return entIds;
        }

        /// <summary>
        /// 获取直线的定数等分点
        /// </summary>
        /// <param name="line">直线</param>
        /// <param name="divNum">等分数</param>
        /// <returns>等分点列表</returns>
        public static List<Point3d> GetBaseLineDivPoints(Line line,int divNum)
        {
            return GetBaseLineDivPoints(line, line.Length / divNum);
        }

        /// <summary>
        /// 获取直线的定距等分点列表
        /// </summary>
        /// <param name="line">直线</param>
        /// <param name="distance">距离</param>
        /// <returns>等分点列表</returns>
        public static List<Point3d> GetBaseLineDivPoints(Line line, double distance)
        {
            List<Point3d> points = new List<Point3d>();
            int divNum =(int) (line.Length / distance);
            double angle = line.Angle;
            for(int i = 0; i < divNum + 1; i++)
            {
                points.Add(PolarPoint(line.StartPoint, distance*i, angle));
            }
            if (distance * divNum != line.Length)
            {
                points.Add(line.EndPoint);
            }
            return points;
        }

        /// <summary>
        /// 获取Polar点坐标
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="dist">终点到起点的距离</param>
        /// <param name="angle">起点到终点的线与X轴正方向的夹角</param>
        /// <returns>Point3d</returns>
        public static Point3d PolarPoint(Point3d startPoint,double dist,double angle)
        {
            double X = startPoint.X + dist * Math.Cos(angle.DegreeToRadian());
            double Y = startPoint.Y + dist * Math.Sin(angle.DegreeToRadian());
            return new Point3d(X, Y, 0);
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="db"></param>
        /// <param name="entId"></param>
        /// <returns></returns>
        public static Entity GetEntity(Database db, ObjectId entId)
        {
            Entity ent;

            using(Transaction tr = db.TransactionManager.StartTransaction())
            {
                ent = entId.GetObject(OpenMode.ForRead) as Entity;
            }
            return ent;
        }

        /// <summary>
        /// 删除图形对象列表
        /// </summary>
        /// <param name="ents">列表</param>
        public static void DeleteEntitys(Entity[] ents)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using(Transaction tr = db.TransactionManager.StartTransaction())
            {
                for(int i = 0; i < ents.Length; i++)
                {
                    Entity ent = ents[i].ObjectId.GetObject(OpenMode.ForWrite) as Entity;
                    ent.Erase();
                }
                tr.Commit();
            }
        }
        /// <summary>
        /// 复制图形对象
        /// </summary>
        /// <param name="entList">图形对象集合</param>
        /// <param name="mt">变换矩阵</param>
        /// <returns>复制后的对象列表</returns>
        public static List<Entity> CopyEntity(List<Entity> entList,Matrix3d mt)
        {
            List<Entity> entListCopy = new List<Entity>();
            for(int i = 0; i < entList.Count; i++)
            {
                entListCopy.Add(entList[i].GetTransformedCopy(mt));
            }
            return entListCopy;
        }
        /// <summary>
        /// 获取集合中的图形对象
        /// </summary>
        /// <param name="ids">ObjectId数组</param>
        /// <returns>图形对象列表</returns>
        private static List<Entity> GetEntity(ObjectId[] ids)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            List<Entity> entList = new List<Entity>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    Entity ent = ids[i].GetObject(OpenMode.ForRead) as Entity;
                    entList.Add(ent);
                }
            }
            return entList;
                
        }
        /// <summary>
        /// 改变图形对象列表中所有对象的颜色
        /// </summary>
        /// <param name="entList">图形对象列表</param>
        private static void LowColorEntity(List<Entity> entList,byte colorIndex )
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using(Transaction tr = db.TransactionManager.StartTransaction())
            {
                for(int i = 0; i < entList.Count; i++)
                {
                    Entity ent = entList[i].ObjectId.GetObject(OpenMode.ForWrite) as Entity;
                    ent.ColorIndex = colorIndex;
                }
                tr.Commit();
            }
        }
        private static void ChangeColor(List<ObjectId> ids)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using(Transaction tr = db.TransactionManager.StartTransaction())
            {
                for(int i = 0; i < ids.Count; i++)
                {
                    Entity ent = ids[i].GetObject(OpenMode.ForWrite) as Entity;
                    ent.ColorIndex = 4;
                }
                tr.Commit();
            }
        }
        private static List<Point3d> GetPoint(SelectionSet sSet)
        {
            List<Point3d> points = new List<Point3d>();
            ObjectId[] ids = sSet.GetObjectIds();
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    Entity ent = ids[i].GetObject(OpenMode.ForRead) as Entity;
                    Point3d center = (ent as Circle).Center;
                    double radius = (ent as Circle).Radius;
                    points.Add(new Point3d(center.Y + radius, center.Y, center.Z));
                }
                tr.Commit();
            }
            return points;
        }
        private static void ChangeColor(SelectionSet sSet)
        {
            ObjectId[] ids = sSet.GetObjectIds();
            Database db = HostApplicationServices.WorkingDatabase;
            using(Transaction tr = db.TransactionManager.StartTransaction())
            {
                for(int i = 0; i < ids.Length; i++)
                {
                    Entity ent = ids[i].GetObject(OpenMode.ForWrite) as Entity;
                    ent.ColorIndex = 1;
                }
                tr.Commit();
            }
        }

    }

}
