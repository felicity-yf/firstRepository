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


namespace CADTOOL
{
    public static class BaseTool
    {
        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="degree">角度值</param>
        /// <returns>弧度</returns>
        public static double DegreeToRadian(this double degree)
        {
            return degree * Math.PI / 180;
        }

        /// <summary>
        /// 弧度转角度
        /// </summary>
        /// <param name="randian">弧度值</param>
        /// <returns>角度</returns>
        public static double RadianToDegree(this double randian)
        {
            return randian * 180 / Math.PI;
        }

        /// <summary>
        /// 判断三点是否在一条直线上
        /// </summary>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <param name="point3">第三个点</param>
        /// <returns>是返加真，否返回假</returns>
        public static bool IsOnOneLine(this Point3d point1, Point3d point2, Point3d point3)
        {
            Vector3d v21 = point2.GetVectorTo(point1);
            Vector3d v23 = point2.GetVectorTo(point3);
            if (v21.GetAngleTo(v23) == 0 || v21.GetAngleTo(v23) == Math.PI)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// 获取两点直线与X的夹角
        /// </summary>
        /// <param name="startPoint">起始点</param>
        /// <param name="endPoint">终止点</param>
        /// <returns>弧度夹角值</returns>
        public static double GetAngleToXAxis(this Point3d startPoint, Point3d endPoint)
        {
            Vector3d XVector = new Vector3d(1, 0, 0);
            Vector3d SToEVector = startPoint.GetVectorTo(endPoint);
            return SToEVector.Y > 0 ? XVector.GetAngleTo(SToEVector) : -XVector.GetAngleTo(SToEVector);
        }

        /// <summary>
        /// 获取两点之间的距离
        /// </summary>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <returns>两点之间的距离</returns>
        public static double GetDistanceBetweenTwoPoint(this Point3d point1, Point3d point2)
        {
            return Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2) + Math.Pow(point1.Z - point2.Z, 2));
        }

        /// <summary>
        /// 获取两点之间的中点
        /// </summary>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <returns>中点</returns>
        public static Point3d GetMidPointBetweenTwoPoint(this Point3d point1, Point3d point2)
        {
            return new Point3d((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2, (point1.Z + point2.Z) / 2);
        }

        /// <summary>
        /// 获取两点直线的斜率
        /// </summary>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <returns>两点直线的斜率</returns>
        public static double GetSlopeBetweenTwoPoint(this Point3d point1, Point3d point2)
        {
            if ((point1.X - point2.X) == 0)
            {
                return double.NaN;
            }
            return (point1.Y - point2.Y) / (point1.X - point2.X);
        }

        /// <summary>
        /// 获取三点的外心
        /// </summary>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <param name="point3">第三个点</param>
        /// <returns>外心</returns>
        public static Point3d GetCircumcenter(this Point3d point1, Point3d point2, Point3d point3)
        {
            Point3d midPiont21 = BaseTool.GetMidPointBetweenTwoPoint(point1, point2);
            Point3d midPiont23 = BaseTool.GetMidPointBetweenTwoPoint(point3, point2);

            double slopeP21 = BaseTool.GetSlopeBetweenTwoPoint(point1, point2);
            double slopeP23 = BaseTool.GetSlopeBetweenTwoPoint(point3, point2);
            if (slopeP21 == slopeP23)
            {
                return Point3d.Origin;
            }
            double centerX = (slopeP21 * slopeP23 * (midPiont23.Y - midPiont21.Y) - slopeP23 * midPiont21.X + slopeP21 * midPiont23.X) / (slopeP21 - slopeP23);
            double centerY = (-1 / slopeP21) * (centerX - midPiont21.X) + midPiont21.Y;
            return new Point3d(centerX, centerY, 0);
        }


    }
    public static class AddEntityTool
    {
        /// <summary>
        /// 将图形对象添加到图形文件中
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="ent">图形实体</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddEntityToModelSpace(this Database db, Entity ent)
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
        /// 将多个图形对象添加到图形文件中
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="ents">图形实体列表</param>
        /// <returns>添加后对象的ObjectId列表</returns>
        public static List<ObjectId> AddEntityToModelSpace(this Database db, List<Entity> ents)
        {
            List<ObjectId> entsId = new List<ObjectId>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                foreach (Entity ent in ents)
                {
                    entsId.Add(btr.AppendEntity(ent));
                    tr.AddNewlyCreatedDBObject(ent, true);
                }
                tr.Commit();
            }
            return entsId;
        }

        /// <summary>
        /// 将多个图形对象添加到图形文件中
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="ents">不定个数的图形对象</param>
        /// <returns>添加后对象的ObjectId数组</returns>
        public static ObjectId[] AddEntityToModelSpace(this Database db, params Entity[] ents)
        {
            ObjectId[] entsId = new ObjectId[ents.Length];
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                for (int i = 0; i < ents.Length; i++)
                {
                    entsId[i] = btr.AppendEntity(ents[i]);
                    tr.AddNewlyCreatedDBObject(ents[i], true);
                }
                tr.Commit();
            }
            return entsId;

        }

        /// <summary>
        /// 通过两个点添加一条直线到图形文件
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="startX">第一个点的x</param>
        /// <param name="startY">第一个点的y</param>
        /// <param name="endX">第二个点的x</param>
        /// <param name="endY">第二个点的y</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddLineToModelSpace(this Database db, double startX, double startY, double endX, double endY)
        {
            ObjectId entId = ObjectId.Null;
            Line line = new Line(new Point3d(startX, startY, 0), new Point3d(endX, endY, 0));
            return AddEntityToModelSpace(db, line);
        }

        /// <summary>
        /// 通过两个点添加一条直线到图形文件
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="startPoint">第一个点</param>
        /// <param name="endPoint">第二个点</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddLineToModelSpace(this Database db, Point3d startPoint, Point3d endPoint)
        {
            ObjectId entId = ObjectId.Null;
            Line line = new Line(startPoint, endPoint);
            return AddEntityToModelSpace(db, line);
        }

        /// <summary>
        /// 通过起点长度以及与X轴正方向的角度创建直线
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="startPoint">起始点</param>
        /// <param name="length">长度</param>
        /// <param name="degree">与X轴正方向的夹角</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddLineToModelSpace(this Database db, Point3d startPoint, double length, double degree)
        {
            double X = startPoint.X + length * Math.Cos(degree.DegreeToRadian());
            double Y = startPoint.Y + length * Math.Sin(degree.DegreeToRadian());
            Point3d endPoint = new Point3d(X, Y, 0);
            return AddLineToModelSpace(db, startPoint, endPoint);
        }

        /// <summary>
        /// 通过圆心半径起始角终止角创建圆弧
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="center">圆心</param>
        /// <param name="radius">半径</param>
        /// <param name="startDegree">起始角</param>
        /// <param name="endDegree">终止角</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddArcToModelSpace(this Database db, Point3d center, double radius, double startDegree, double endDegree)
        {
            return AddEntityToModelSpace(db, new Arc(center, radius, startDegree.DegreeToRadian(), endDegree.DegreeToRadian()));
        }

        /// <summary>
        /// 通过圆弧对象直接创建圆弧
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="arc">圆弧对象</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddArcToModelSpace(this Database db, Arc arc)
        {
            return AddEntityToModelSpace(db, arc);
        }

        /// <summary>
        /// 通过三个点创建圆弧
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="startPoint">起始点</param>
        /// <param name="midPoint">中间点</param>
        /// <param name="endPoint">终止点</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddArcToModelSpace(this Database db, Point3d startPoint, Point3d midPoint, Point3d endPoint)
        {
            if (BaseTool.IsOnOneLine(startPoint, midPoint, endPoint))
            {
                return ObjectId.Null;
            }
            CircularArc3d cArc = new CircularArc3d(startPoint, midPoint, endPoint); //用于计算的圆弧对象

            Vector3d cs = cArc.Center.GetVectorTo(startPoint);               //起点到圆心的向量
            Vector3d ce = cArc.Center.GetVectorTo(endPoint);                 //终点到圆心的向量
            Vector3d xVector = new Vector3d(1, 0, 0);

            double startAngle = cs.Y > 0 ? xVector.GetAngleTo(cs) : -xVector.GetAngleTo(cs);
            double endAngle = ce.Y > 0 ? xVector.GetAngleTo(ce) : -xVector.GetAngleTo(ce);

            Arc arc = new Arc(cArc.Center, cArc.Radius, startAngle, endAngle);

            return AddArcToModelSpace(db, arc);
        }

        /// <summary>
        /// 通过圆心起点角度创建圆弧
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="center">圆心</param>
        /// <param name="startPoint">起始点</param>
        /// <param name="degree">角度</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddArcToModelSpace(this Database db, Point3d center, Point3d startPoint, double degree)
        {
            double radius = center.GetDistanceBetweenTwoPoint(startPoint);
            double startAngle = center.GetAngleToXAxis(startPoint);

            Arc arc = new Arc(center, radius, startAngle, startAngle + degree.DegreeToRadian());

            return AddArcToModelSpace(db, arc);
        }

        /// <summary>
        /// 通过圆心和半径创建圆
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="center">圆心</param>
        /// <param name="radius">半径</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddCircleToModelSpace(this Database db, Point3d center, double radius)
        {
            return AddEntityToModelSpace(db, new Circle(center, new Vector3d(0, 0, 1), radius));
        }

        /// <summary>
        /// 通过两点创建圆
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddCircleToModelSpace(this Database db, Point3d point1, Point3d point2)
        {
            Point3d center = BaseTool.GetMidPointBetweenTwoPoint(point1, point2);
            double radius = BaseTool.GetDistanceBetweenTwoPoint(center, point1);
            return AddEntityToModelSpace(db, new Circle(center, new Vector3d(0, 0, 1), radius));
        }

        /// <summary>
        /// 通过三点创建圆
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <param name="point3">第三个点</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddCircleToModelSpace(this Database db, Point3d point1, Point3d point2, Point3d point3)
        {
            if (point1.IsOnOneLine(point2, point3))
            {
                return ObjectId.Null;
            }
            CircularArc3d cArc = new CircularArc3d(point1, point2, point3);
            double radius = cArc.Center.GetDistanceBetweenTwoPoint(point1);
            return AddEntityToModelSpace(db, new Circle(cArc.Center, new Vector3d(0, 0, 1), radius));

        }

        /// <summary>
        /// 通过多个点创建多段线，不定参数形式。
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="isClosed">是否闭合</param>
        /// <param name="constantWidth">线宽</param>
        /// <param name="vertices">顶点</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddPolyLineToModelSpace(this Database db, bool isClosed, double constantWidth, params Point2d[] vertices)
        {
            Polyline pLine = new Polyline();
            if (vertices.Length < 2)
            {
                return ObjectId.Null;
            }
            for (int i = 0; i < vertices.Length; i++)
            {
                pLine.AddVertexAt(i, vertices[i], 0, 0, 0);
            }
            pLine.Closed = isClosed;
            pLine.ConstantWidth = constantWidth;
            return AddEntityToModelSpace(db, pLine);
        }
        /// <summary>
        /// 通过多个点创建多段线,列表形式
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="isClosed">是否闭合</param>
        /// <param name="constantWidth">线宽</param>
        /// <param name="vertices">点列表</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddPolyLineToModelSpace(this Database db, bool isClosed, double constantWidth, List<Point2d> vertices)
        {
            if (vertices.Count < 2)
            {
                return ObjectId.Null;
            }
            Polyline pLine = new Polyline();
            foreach (Point2d point in vertices)
            {
                pLine.AddVertexAt(vertices.IndexOf(point), point, 0, 0, 0);
            }
            pLine.Closed = isClosed;
            pLine.ConstantWidth = constantWidth;
            return AddEntityToModelSpace(db, pLine);
        }

        /// <summary>
        /// 创建多条多段线
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="pLines">多段线对象</param>
        /// <returns>多段线ObjectId列表</returns>
        public static List<ObjectId> AddPolyLineToModelSpace(this Database db, List<Polyline> pLines)
        {
            List<ObjectId> entsId = new List<ObjectId>();
            foreach (Polyline pLine in pLines)
            {
                entsId.Add(AddEntityToModelSpace(db, pLine));
            }
            return entsId;
        }

        /// <summary>
        /// 通过两点创建矩形
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="leftPoint">左边的点</param>
        /// <param name="rightPoint">右边的点</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddRectToModelSpace(this Database db, Point2d leftPoint, Point2d rightPoint)
        {

            if (leftPoint.Y > rightPoint.Y)
            {
                Point2d leftUpPoint = leftPoint;
                Point2d rightLowPoint = rightPoint;
                Point2d leftLowPoint = new Point2d(leftPoint.X, rightPoint.Y);
                Point2d rightUpPoint = new Point2d(rightPoint.X, leftPoint.Y);
                return AddPolyLineToModelSpace(db, true, 0, leftUpPoint, rightUpPoint, rightLowPoint, leftLowPoint);
            }
            else
            {
                Point2d leftLowPoint = leftPoint;
                Point2d rightUpPoint = rightPoint;
                Point2d leftUpPoint = new Point2d(leftLowPoint.X, rightUpPoint.Y);
                Point2d rightLowPoint = new Point2d(rightUpPoint.X, leftLowPoint.Y);
                return AddPolyLineToModelSpace(db, true, 0, leftUpPoint, rightUpPoint, rightLowPoint, leftLowPoint);
            }

        }

        /// <summary>
        /// 通过圆心，半径，边数，角度创建外接正多边形
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="center">圆心</param>
        /// <param name="radius">半径</param>
        /// <param name="edges">边数</param>
        /// <param name="degree">旋转角</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddPolygonToModelSpace(this Database db, Point2d center, double radius, int edges, double degree)
        {
            if (edges < 3)
            {
                return ObjectId.Null;
            }
            List<Point2d> points = new List<Point2d>();
            double startDegree = 0;
            double stepDegree = 360 / edges;
            for (int i = 0; i < edges; i++)
            {
                double x = radius * Math.Cos((startDegree + stepDegree * i + degree + 90).DegreeToRadian()) + center.X;
                double y = radius * Math.Sin((startDegree + stepDegree * i + degree + 90).DegreeToRadian()) + center.Y;
                points.Add(new Point2d(x, y));

            }
            return AddEntityTool.AddPolyLineToModelSpace(db, true, 0, points);
        }

        /// <summary>
        /// 通过中心,长短轴,起止角,旋转角创建椭圆
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="center">椭圆心</param>
        /// <param name="majorRadius">长轴</param>
        /// <param name="shortRadius">短轴</param>
        /// <param name="startAngel">起始角</param>
        /// <param name="endAngle">终止角</param>
        /// <param name="degree">长轴与X轴正方向的夹角</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddEllipseToModelSpace(this Database db, Point3d center, double majorRadius, double shortRadius, double startAngle = 0, double endAngle = 360, double degree = 0)
        {
            double ratio = shortRadius / majorRadius;
            Vector3d majorAxis = new Vector3d(majorRadius * Math.Cos(degree.DegreeToRadian()), majorRadius * Math.Sin(degree.DegreeToRadian()), 0);
            Ellipse el = new Ellipse(center, Vector3d.ZAxis, majorAxis, ratio, startAngle.DegreeToRadian(), endAngle.DegreeToRadian());
            return AddEntityToModelSpace(db, el);
        }

        /// <summary>
        /// 通过两点和半轴长,起止角绘制椭圆
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="majorPoint1">第一个点</param>
        /// <param name="majorPoint2">第二个点</param>
        /// <param name="shortRadius">半轴长</param>
        /// <param name="startAngle">起始角</param>
        /// <param name="endAngle">终止角</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddEllipseToModelSpace(this Database db, Point3d majorPoint1, Point3d majorPoint2, double shortRadius, double startAngle = 0, double endAngle = 360)
        {
            Point3d center = majorPoint1.GetMidPointBetweenTwoPoint(majorPoint2);
            //短轴与长轴比
            double ratio = 2 * shortRadius / majorPoint1.GetDistanceBetweenTwoPoint(majorPoint2);
            Vector3d majorAxis = majorPoint2.GetVectorTo(center);
            Ellipse el = new Ellipse(center, Vector3d.ZAxis, majorAxis, ratio, startAngle.DegreeToRadian(), endAngle.DegreeToRadian());
            return AddEntityToModelSpace(db, el);
        }

        /// <summary>
        /// 通过两点创建与两点构成的矩形内切的椭圆
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <returns>添加后对象的ObjectId</returns>
        public static ObjectId AddEllipseToModelSpace(this Database db, Point3d point1, Point3d point2)
        {
            Point3d center = point1.GetMidPointBetweenTwoPoint(point2);
            if (Math.Max(Math.Abs(point1.Y - point2.Y), Math.Abs(point1.X - point2.X)) == Math.Abs(point1.Y - point2.Y))
            {
                double ratio = Math.Abs((point1.X - point2.X) / (point1.Y - point2.Y));
                Vector3d majorVector = new Vector3d(0, Math.Abs(point1.Y - point2.Y) / 2, 0);
                Ellipse el = new Ellipse(center, Vector3d.ZAxis, majorVector, ratio, 0, 2 * Math.PI);
                return AddEntityToModelSpace(db, el);
            }
            else
            {
                double ratio = Math.Abs((point1.Y - point2.Y) / (point1.X - point2.X));
                Vector3d majorVector = new Vector3d(Math.Abs(point1.X - point2.X) / 2, 0, 0);
                Ellipse el = new Ellipse(center, Vector3d.ZAxis, majorVector, ratio, 0, 2 * Math.PI);
                return AddEntityToModelSpace(db, el);
            }

        }


    }

    public static class HatchTool
    {
        public struct HatchPattern
        {
            public static readonly string SOLID = "SOLID";
            public static readonly string ANGLE = "ANGLE";
            public static readonly string ANSI31 = "ANSI31";
            public static readonly string ANSI32 = "ANSI32";
            public static readonly string ANSI33 = "ANSI33";
            public static readonly string ANSI34 = "ANSI34";
            public static readonly string ANSI35 = "ANSI35";
            public static readonly string ANSI36 = "ANSI36";
            public static readonly string ANSI37 = "ANSI37";
            public static readonly string ANSI38 = "ANSI38";
            public static readonly string AR_B816 = "AR-B816";
            public static readonly string AR_B816C = "AR-B816C";
            public static readonly string AR_B88 = "AR-B88";
            public static readonly string AR_BRELM = "AR-BRELM";
            public static readonly string AR_BRSTD = "AR-BSTD";
            public static readonly string AR_CONC = "AR-CONC";
            public static readonly string AR_HBONE = "AR-PARQ1";
            public static readonly string AR_RROOF = "AR-RROOF";
            public static readonly string AR_RSHKE = "AR-RSHKE";
            public static readonly string AR_SAND = "AR-SAND";
            public static readonly string BOX = "BOX";
            public static readonly string BRASS = "BRASS";
            public static readonly string BRICK = "BRICK";
            public static readonly string BRSTONE = "BRTONE";
            public static readonly string CLAY = "CLAY";
            public static readonly string CORK = "CORK";
            public static readonly string CROSS = "CROSS";
            public static readonly string DASH = "DASH";
            public static readonly string DOLMIT = "DOLMIT";
            public static readonly string DOTS = "DOTS";
            public static readonly string EARTH = "EARTH";
            public static readonly string ESCHER = "ESCHER";
            public static readonly string FLEX = "FLEX";
            public static readonly string GOST_GLASS = "GOST_GLASS";
            public static readonly string GOST_GROUND = "GOST_GROUND";
            public static readonly string GOST_WOOD = "GOST_WOOD";
            public static readonly string GRASS = "GRASS";
            public static readonly string GRATE = "GRATE";
            public static readonly string GRAVEL = "GRAVEL";
            public static readonly string HEX = "HEX";
            public static readonly string HONEY = "HONEY";
            public static readonly string HOUND = "HOUND";
            public static readonly string INSUL = "INSUL";
            public static readonly string NET = "NET";
            public static readonly string NET3 = "NET3";
            public static readonly string STARS = "STARS";
        }

        public struct HatchGradientPattern
        {
            public static readonly string GR_LINEAR = "Linear";
            public static readonly string GR_CYLIN = "Cylinder";
            public static readonly string GR_INVCYL = "Invcylinder";
            public static readonly string GR_SPHER = "Spherical";
            public static readonly string GR_HEMISP = "Hemispherical";
            public static readonly string GR_CURVED = "Curved";
            public static readonly string GR_INVSPH = "Invspherical";
            public static readonly string GR_INVHEM = "Invhemispherical";
            public static readonly string GR_INVCUR = "Invcurved";
        }
        /// <summary>
        /// 图案填充
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="patternName">图案名</param>
        /// <param name="entId">边界图形ObjectId</param>
        /// <param name="backgroundColor">背景色</param>
        /// <param name="hatchColorIndex">前景色索引</param>
        /// <param name="patternAngle">填充角度</param>
        /// <param name="patternScale">填充比例</param>
        /// <returns>对象的ObjectId</returns>
        public static ObjectId HatchEntity(this Database db, string patternName, ObjectId entId, Color backgroundColor, int hatchColorIndex = 0, double patternAngle = 45, double patternScale = 5)
        {
            ObjectId hatchId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Hatch hatch = new Hatch();
                hatch.PatternScale = patternScale; //填充比例
                hatch.BackgroundColor = backgroundColor;
                hatch.ColorIndex = hatchColorIndex;
                hatch.SetHatchPattern(HatchPatternType.PreDefined, patternName); //填充类型和图案名
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                hatchId = btr.AppendEntity(hatch);
                tr.AddNewlyCreatedDBObject(hatch, true);

                hatch.PatternAngle = patternAngle.DegreeToRadian();//填充角度
                hatch.Associative = true; //设置关联

                ObjectIdCollection obIds = new ObjectIdCollection();
                obIds.Add(entId);
                hatch.AppendLoop(HatchLoopTypes.Outermost, obIds);//设置边界图形和填充方式
                hatch.EvaluateHatch(true); //计算填充并显示


                tr.Commit();
            }
            return hatchId;
        }
        /// <summary>
        /// 图案填充
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="patternName">图案名称</param>
        /// <param name="hatchLoopTypes"></param>
        /// <param name="entsId">图案边界ObjectId列表</param>
        /// <param name="backgroundColor">背景色</param>
        /// <param name="hatchColorIndex">前景色索引</param>
        /// <param name="patternAngle">填充角度</param>
        /// <param name="patternScale">填充比例</param>
        /// <returns>填充对象的ObjectId</returns>
        public static ObjectId HatchEntity(this Database db, string patternName, List<HatchLoopTypes> hatchLoopTypes, List<ObjectId> entsId, Color backgroundColor, int hatchColorIndex = 0, double patternAngle = 45, double patternScale = 5)
        {
            ObjectId hatchId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Hatch hatch = new Hatch();
                hatch.PatternScale = patternScale; //填充比例
                hatch.BackgroundColor = backgroundColor;
                hatch.ColorIndex = hatchColorIndex;
                hatch.SetHatchPattern(HatchPatternType.PreDefined, patternName); //填充类型和图案名
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                hatchId = btr.AppendEntity(hatch);
                tr.AddNewlyCreatedDBObject(hatch, true);

                hatch.PatternAngle = patternAngle.DegreeToRadian();//填充角度
                hatch.Associative = true; //设置关联

                ObjectIdCollection obIds = new ObjectIdCollection();

                foreach (ObjectId entId in entsId)
                {
                    obIds.Clear();
                    obIds.Add(entId);
                    hatch.AppendLoop(hatchLoopTypes[entsId.IndexOf(entId)], obIds);//设置边界图形和填充方式
                }
                hatch.EvaluateHatch(true); //计算填充并显示
                tr.Commit();
            }
            return hatchId;
        }

        /// <summary>
        /// 渐变填充
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="gradientPattern">图案名称</param>
        /// <param name="colorIndex1">第一个颜色</param>
        /// <param name="colorIndex2">第二个颜色</param>
        /// <param name="entId">要填充对象的ObjectId</param>
        /// <param name="gradientAngle">填充角度</param>
        /// <returns>填充对象的ObjectId</returns>
        public static ObjectId HatchGradient(this Database db, string gradientPattern, short colorIndex1, short colorIndex2, ObjectId entId, double gradientAngle)
        {
            ObjectId hatchId = ObjectId.Null;
            ObjectIdCollection entsId = new ObjectIdCollection();
            entsId.Add(entId);
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Hatch hatch = new Hatch();
                hatch.HatchObjectType = HatchObjectType.GradientObject; //设置渐变填充
                hatch.SetGradient(GradientPatternType.PreDefinedGradient, gradientPattern); //设置填充类型和图案名称
                hatch.GradientAngle = gradientAngle.DegreeToRadian();

                //设置填充色
                Color color1 = Color.FromColorIndex(ColorMethod.ByColor, colorIndex1);
                Color color2 = Color.FromColorIndex(ColorMethod.ByColor, colorIndex2);
                GradientColor gradientColor1 = new GradientColor(color1, 0);
                GradientColor gradientColor2 = new GradientColor(color2, 1);
                hatch.SetGradientColors(new GradientColor[] { gradientColor1, gradientColor2 });

                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                hatchId = btr.AppendEntity(hatch);
                tr.AddNewlyCreatedDBObject(hatch, true);
                //添加关联
                hatch.Associative = true;
                hatch.AppendLoop(HatchLoopTypes.Outermost, entsId);
                //计算并显示
                hatch.EvaluateHatch(true);

                tr.Commit();

            }
            return hatchId;

        }



    }

    public static partial class EditEntityTool
    {
        /// <summary>
        /// 更改图形颜色
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="entId">图形ObjectId</param>
        /// <param name="colorIndex">颜色值索引</param>
        public static void ChangeEntityColor(this Database db, ObjectId entId, short colorIndex)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                Entity ent = entId.GetObject(OpenMode.ForWrite) as Entity;
                ent.ColorIndex = colorIndex;
                tr.Commit();
            }
        }
        /// <summary>
        /// 改变图形颜色
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="entId">图形的ObjectId</param>
        /// <param name="colorIndex">颜色值索引</param>
        public static void ChangeEntityColor(this Database db, Entity entId, short colorIndex)
        {
            if (entId.IsNewObject)
            {
                entId.ColorIndex = colorIndex;
            }
            else
            {
                ChangeEntityColor(db, entId, colorIndex);
            }

        }

        /// <summary>
        /// 移动图形
        /// </summary>
        /// <param name="db"></param>
        /// <param name="entId">图形的ObjectId</param>
        /// <param name="sourcePoint">参考原点</param>
        /// <param name="targetPoint">参考目标点</param>
        public static void MoveEntity(this Database db, ObjectId entId, Point3d sourcePoint, Point3d targetPoint)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable; //打开块表
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord; //打开块表记录
                                                                                                                            //Entity ent = tr.GetObject(entId, OpenMode.ForWrite) as Entity;
                Entity ent = entId.GetObject(OpenMode.ForWrite) as Entity; //打开图形
                Vector3d vector = sourcePoint.GetVectorTo(targetPoint); //计算变换矩阵
                Matrix3d mt = Matrix3d.Displacement(vector);
                ent.TransformBy(mt);
                tr.Commit();
            }
        }

        /// <summary>
        /// 移动图形
        /// </summary>
        /// <param name="db"></param>
        /// <param name="ent">图形Entity</param>
        /// <param name="sourcePoint">参考原点</param>
        /// <param name="targetPoint">参考目标点</param>
        public static void MoveEntity(this Database db, Entity ent, Point3d sourcePoint, Point3d targetPoint)
        {
            // 判断图形的对象是不是新图
            if (ent.IsNewObject)
            {
                Vector3d vector = sourcePoint.GetVectorTo(targetPoint);
                Matrix3d mt = Matrix3d.Displacement(vector);
                ent.TransformBy(mt);
            }
            else
            {
                MoveEntity(db, ent.ObjectId, sourcePoint, targetPoint);
            }
        }
        /// <summary>
        /// 复制图形
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="entId">要复制的图形的ObjectId</param>
        /// <param name="sourcePoint">参考原点</param>
        /// <param name="targetPoint">参考目标点</param>
        /// <returns>复制后的Entity</returns>

        public static Entity CopyEntity(this Database db, ObjectId entId, Point3d sourcePoint, Point3d targetPoint)
        {
            //声明图形对象
            Entity entCopy;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable; //打开块表
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord; //打开块表记录
                                                                                                                            //Entity ent = tr.GetObject(entId, OpenMode.ForWrite) as Entity;
                Entity ent = entId.GetObject(OpenMode.ForWrite) as Entity; //打开图形
                Vector3d vector = sourcePoint.GetVectorTo(targetPoint); //计算变换矩阵
                Matrix3d mt = Matrix3d.Displacement(vector);
                entCopy = ent.GetTransformedCopy(mt);
                btr.AppendEntity(entCopy);
                tr.AddNewlyCreatedDBObject(entCopy, true);
                tr.Commit();
            }
            return entCopy;
        }

        /// <summary>
        /// 复制图形
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="ent">要复制的Eneity</param>
        /// <param name="sourcePoint">参考原点</param>
        /// <param name="targetPoint">参考目标点</param>
        /// <returns>复制后的Entity</returns>
        public static Entity CopyEntity(this Database db, Entity ent, Point3d sourcePoint, Point3d targetPoint)
        {
            Entity entCopy;
            if (ent.IsNewObject)
            {
                Vector3d vector = sourcePoint.GetVectorTo(targetPoint);
                Matrix3d mt = Matrix3d.Displacement(vector);
                entCopy = ent.GetTransformedCopy(mt);
                db.AddEntityToModelSpace(entCopy);

            }
            else
            {
                entCopy = CopyEntity(db, ent.ObjectId, sourcePoint, targetPoint);
            }
            return entCopy;
        }

        /// <summary>
        /// 旋转图形
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="entId">要旋转的图形的ObjectId</param>
        /// <param name="center">旋转中心</param>
        /// <param name="degree">旋转角度</param>
        public static void RotateEntity(this Database db, ObjectId entId, Point3d center, double degree)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable; //打开块表
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord; //打开块表记录
                                                                                                                            //Entity ent = tr.GetObject(entId, OpenMode.ForWrite) as Entity;
                Entity ent = entId.GetObject(OpenMode.ForWrite) as Entity; //打开图形

                Matrix3d mt = Matrix3d.Rotation(degree.DegreeToRadian(), Vector3d.ZAxis, center);
                ent.TransformBy(mt);
                tr.Commit();
            }
        }

        /// <summary>
        /// 旋转图形
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="ent">要旋转的图形的Entity</param>
        /// <param name="center">旋转中心</param>
        /// <param name="degree">旋转角度</param>
        public static void RotateEntity(this Database db, Entity ent, Point3d center, double degree)
        {

            if (ent.IsNewObject)
            {

                Matrix3d mt = Matrix3d.Rotation(degree.DegreeToRadian(), Vector3d.ZAxis, center);
                ent.TransformBy(mt);

            }
            else
            {
                RotateEntity(db, ent.ObjectId, center, degree);
            }
        }

        /// <summary>
        /// 镜像图形
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="entId">要镜像的图形ObjectId</param>
        /// <param name="point1">镜像参考点一</param>
        /// <param name="point2">镜像参考点一</param>
        /// <param name="isEraseSource">是否删除原图</param>
        /// <returns>镜像对象的Entity</returns>
        public static Entity MirrorEntity(this Database db, ObjectId entId, Point3d point1, Point3d point2, bool isEraseSource = false)
        {
            //声明图形对象用于返回
            Entity entCopy;
            //计算镜像的变换矩阵
            Matrix3d mt = Matrix3d.Mirroring(new Line3d(point1, point2));
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(entId, OpenMode.ForWrite) as Entity;
                entCopy = ent.GetTransformedCopy(mt);
                db.AddEntityToModelSpace(entCopy);
                //是否删除原图
                if (isEraseSource)
                {
                    ent.Erase();
                }
                tr.Commit();
            }
            return entCopy;
        }

        /// <summary>
        /// 镜像图形
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="ent">要镜像的图形</param>
        /// <param name="point1">镜像参考点一</param>
        /// <param name="point2">镜像参考点一</param>
        /// <param name="isEraseSource">是否删除原图</param>
        /// <returns>镜像对象的Entity</returns>
        public static Entity MirrorEntity(this Database db, Entity ent, Point3d point1, Point3d point2, bool isEraseSource = false)
        {
            Entity entCopy;
            if (ent.IsNewObject)
            {
                Matrix3d mt = Matrix3d.Mirroring(new Line3d(point1, point2));
                entCopy = ent.GetTransformedCopy(mt);
                db.AddEntityToModelSpace(entCopy);
            }
            else
            {
                entCopy = MirrorEntity(db, ent.ObjectId, point1, point2, isEraseSource);
            }
            return entCopy;
        }

        /// <summary>
        /// 缩放图形
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="entId">要缩放的图形ObjectId</param>
        /// <param name="basePoint">参考点</param>
        /// <param name="factor">缩放比例</param>
        public static void ScaleEntity(this Database db, ObjectId entId, Point3d basePoint, double factor)
        {
            Matrix3d mt = Matrix3d.Scaling(factor, basePoint);
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                //打开要缩放的图形对象
                Entity ent = entId.GetObject(OpenMode.ForWrite) as Entity;
                ent.TransformBy(mt);
                tr.Commit();
            }
        }

        /// <summary>
        /// 缩放图形
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="ent">要缩放的图形Entity</param>
        /// <param name="basePoint">参考点</param>
        /// <param name="factor">缩放比例</param>
        public static void ScaleEntity(this Database db, Entity ent, Point3d basePoint, double factor)
        {
            if (ent.IsNewObject)
            {
                Matrix3d mt = Matrix3d.Scaling(factor, basePoint);
                ent.TransformBy(mt);
            }
            else
            {
                ScaleEntity(db, ent.ObjectId, basePoint, factor);
            }
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="entId">所要删除对象的ObjectId</param>
        public static void EraseEntity(this Database db, ObjectId entId)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ent = entId.GetObject(OpenMode.ForWrite) as Entity;
                ent.Erase();
                tr.Commit();
            }
        }

        /// <summary>
        /// 矩形阵列
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="entId">要阵列的图形的ObjectId</param>
        /// <param name="row">行数</param>
        /// <param name="col">列数</param>
        /// <param name="rowGap">行间距</param>
        /// <param name="colGap">列间距</param>
        /// <param name="degree">旋转角</param>
        /// <returns>所阵列出的图形的Entity列表</returns>
        public static List<Entity> ArrayRectEntity(this Database db, ObjectId entId, int row, int col, double rowGap, double colGap)
        {
            List<Entity> entCopy = new List<Entity>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                Entity ent = entId.GetObject(OpenMode.ForWrite) as Entity;
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        Matrix3d mtDisplacement = Matrix3d.Displacement(new Vector3d(j * colGap, i * rowGap, 0));
                        Entity entA = ent.GetTransformedCopy(mtDisplacement);    //获取变换后的对象

                        btr.AppendEntity(entA);
                        tr.AddNewlyCreatedDBObject(entA, true);
                        entCopy.Add(entA);
                    }
                }
                ent.Erase(); //删除原始的图形
                tr.Commit();
            }
            return entCopy;
        }

        /// <summary>
        /// 矩形阵列
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="ent">要阵列的图形的Entity</param>
        /// <param name="row">行数</param>
        /// <param name="col">列数</param>
        /// <param name="rowGap">行间距</param>
        /// <param name="colGap">列间距</param>
        /// <param name="degree">旋转角</param>
        /// <returns>所阵列出的图形的Entity列表</returns>
        public static List<Entity> ArrayRectEntity(this Database db, Entity ent, int row, int col, double rowGap, double colGap)
        {
            List<Entity> entCopy = new List<Entity>();
            if (ent.IsNewObject)
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    for (int i = 0; i < row; i++)
                    {
                        for (int j = 0; j < col; j++)
                        {
                            Matrix3d mtDisplacement = Matrix3d.Displacement(new Vector3d(j * colGap, i * rowGap, 0));
                            Entity entA = ent.GetTransformedCopy(mtDisplacement);    //获取变换后的对象

                            btr.AppendEntity(entA);
                            tr.AddNewlyCreatedDBObject(entA, true);
                            entCopy.Add(entA);
                        }
                    }
                    tr.Commit();
                }

                return entCopy;
            }
            else
            {
                return ArrayRectEntity(db, ent.ObjectId, row, col, rowGap, colGap);
            }
        }
        /// <summary>
        /// 环形阵列
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="entId">要阵列对象的ObjectId</param>
        /// <param name="count">要阵列的个数</param>
        /// <param name="degree">阵列覆盖的角度</param>
        /// <param name="center">阵列中心点</param>
        /// <returns>阵列表对象的列表</returns>
        public static List<Entity> ArrayRingEntity(this Database db, ObjectId entId, int count, double degree, Point3d center)
        {
            List<Entity> entCopy = new List<Entity>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                Entity ent = entId.GetObject(OpenMode.ForWrite) as Entity;
                //限定阵列角度
                degree = degree > 360 ? 360 : degree;
                degree = degree < -360 ? -360 : degree;
                int divAngCount = count - 1;
                if (degree == 360 || degree == -360)
                {
                    divAngCount = count;
                }

                for (int i = 0; i < count; i++)
                {
                    Matrix3d mt = Matrix3d.Rotation((i * degree / divAngCount).DegreeToRadian(), Vector3d.ZAxis, center);
                    Entity entA = ent.GetTransformedCopy(mt);
                    btr.AppendEntity(entA);
                    tr.AddNewlyCreatedDBObject(entA, true);
                    entCopy.Add(entA);
                }
                ent.Erase();
                tr.Commit();
            }
            return entCopy;
        }



    }

    public static partial class PromptTool
    {
        /// <summary>
        /// 获取点
        /// </summary>
        /// <param name="ed">Editor</param>
        /// <param name="promptString">提示字符串</param>
        /// <returns>点提示结果</returns>
        public static PromptPointResult GetPoint2(this Editor ed, string promptString)
        {
            //声明一个获取点的指示类;
            PromptPointOptions ppo = new PromptPointOptions(promptString);
            ppo.AllowNone = true;  //使回车和空格有效
            return ed.GetPoint(ppo);
        }
        /// <summary>
        /// 获取点或关键字
        /// </summary>
        /// <param name="ed">命令行</param>
        /// <param name="promptString">指示词</param>
        /// <param name="pointBase">基准点</param>
        /// <param name="keyWords">关键字</param>
        /// <returns>点提示结果</returns>
        public static PromptPointResult GetPoint(this Editor ed, string promptString, Point3d pointBase, params string[] keyWords)
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
    }

    public class CircleJig : EntityJig
    {

        private double jRadius;
        public CircleJig(Point3d center) : base(new Circle())
        {
            ((Circle)Entity).Center = center;
        }

        /// <summary>
        /// 当鼠标在屏幕上移动时被调用,用于改变图形的属性
        /// </summary>
        /// <param name="prompts"></param>
        /// <returns></returns>
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            //声明jig提示信息
            JigPromptPointOptions jppo = new JigPromptPointOptions("\n请指定圆上的一个点:");
            char space = (char)32;
            jppo.Keywords.Add("U");
            jppo.Keywords.Add(space.ToString());
            jppo.UserInputControls = UserInputControls.Accept3dCoordinates;
            jppo.Cursor = CursorType.RubberBand;  //显示圆心到当前点的虚线            
            jppo.BasePoint = ((Circle)Entity).Center;
            jppo.UseBasePoint = true;
            //获取拖拽时鼠标的位置状态
            PromptPointResult ppr = prompts.AcquirePoint(jppo);
            jRadius = ppr.Value.GetDistanceBetweenTwoPoint(((Circle)Entity).Center);
            return SamplerStatus.NoChange;
        }

        /// <summary>
        /// 用于改新图形对象，不用事务处理
        /// </summary>
        /// <returns></returns>
        protected override bool Update()
        {
            //动态更新圆的半径
            if (jRadius > 0)
            {
                ((Circle)Entity).Radius = jRadius;
                return true;
            }
            return false;
        }

        public Entity GetEntity()
        {
            return Entity;
        }
    }

    public class LineJig : EntityJig
    {
        private Point3d jStartPoint; //直线的起点
        private Point3d jEndPoint;  //直线的终点
        private string jPromptString;  //提示信息
        private string[] jKeywords;   //交互关键字

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="jPromptStr">提示信息</param>
        /// <param name="keywords">交互关键字</param>
        public LineJig(Point3d startPoint, string jPromptStr, string[] keywords) : base(new Line())
        {
            jStartPoint = startPoint;
            ((Line)Entity).StartPoint = jStartPoint;
            jPromptString = jPromptStr;
            jKeywords = keywords;

        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            //声明提示信息类
            JigPromptPointOptions jppo = new JigPromptPointOptions(jPromptString);
            //添加关键字
            for (int i = 0; i < jKeywords.Length; i++)
            {
                jppo.Keywords.Add(jKeywords[i]);
            }
            char space = (char)32;
            jppo.Keywords.Add(space.ToString());
            //设置获取信息类型
            jppo.UserInputControls = UserInputControls.Accept3dCoordinates;
            //取消系统自动添加关键字提示信息
            jppo.AppendKeywordsToMessage = false;
            //jppo.Cursor = CursorType.RubberBand;
            PromptPointResult ppr = prompts.AcquirePoint(jppo);
            jEndPoint = ppr.Value;
            return SamplerStatus.NoChange;

        }

        protected override bool Update()
        {
            ((Line)Entity).EndPoint = jEndPoint;
            return true;
        }
        /// <summary>
        /// 返回图形对象
        /// </summary>
        /// <returns>Entity</returns>
        public Entity GetEntity()
        {
            return Entity;
        }
    }


}
