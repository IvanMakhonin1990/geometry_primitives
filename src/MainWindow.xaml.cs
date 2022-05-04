
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Windows.Markup;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Globalization;
using System.Xml;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;

using System.Windows.Media.Media3D;

namespace Primitives {

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		Rect3D bounds;
		Point3D center;
		bool isInitialized = false;
		private GeometryModel3D mGeometry;
		private bool mDown;
		private Point mLastPos;

		public MainWindow()
		{
			InitializeComponent();
			bounds = new Rect3D(0, 0, 0, 1, 1, 1);
			center = new Point3D();
		}

		private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			camera.Position = new Point3D(camera.Position.X, camera.Position.Y, camera.Position.Z - e.Delta / 250D);
		}

		private void Button_plus(object sender, RoutedEventArgs e)
		{
			var dir = camera.LookDirection;
			dir.Normalize();
			camera.Position = camera.Position + dir;
		}
		private void Button_minus(object sender, RoutedEventArgs e)
		{
			var dir = camera.LookDirection;
			dir.Normalize();
			camera.Position = camera.Position - dir;
		}

		private void Grid_MouseMove(object sender, MouseEventArgs e)
		{
			if (mDown)
			{
				Point pos = Mouse.GetPosition(viewport);
				Point actualPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
				double dx = actualPos.X - mLastPos.X, dy = actualPos.Y - mLastPos.Y;

				double mouseAngle = 0;
				if (dx != 0 && dy != 0)
				{
					mouseAngle = Math.Asin(Math.Abs(dy) / Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));
					if (dx < 0 && dy > 0) mouseAngle += Math.PI / 2;
					else if (dx < 0 && dy < 0) mouseAngle += Math.PI;
					else if (dx > 0 && dy < 0) mouseAngle += Math.PI * 1.5;
				}
				else if (dx == 0 && dy != 0) mouseAngle = Math.Sign(dy) > 0 ? Math.PI / 2 : Math.PI * 1.5;
				else if (dx != 0 && dy == 0) mouseAngle = Math.Sign(dx) > 0 ? 0 : Math.PI;

				double axisAngle = mouseAngle + Math.PI / 2;

				Vector3D axis = new Vector3D(Math.Cos(axisAngle) * 4, Math.Sin(axisAngle) * 4, 0);

				double rotation = 0.01 * Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

				QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(axis, rotation * 180 / Math.PI));
				var tr = new RotateTransform3D(r);
				camera.LookDirection = tr.Transform(camera.LookDirection);
				camera.Position = tr.Transform(camera.Position);

				mLastPos = actualPos;
			}
		}

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton != MouseButtonState.Pressed) return;
			mDown = true;
			Point pos = Mouse.GetPosition(viewport);
			mLastPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
		}

		private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
		{
			mDown = false;
		}

		private void Cylinder_button_Click(object sender, RoutedEventArgs e)
		{
			var form = new Primitives.CreateCylinder();
			form.Ok.Click += new RoutedEventHandler((object s, RoutedEventArgs ea) =>
			{
				double x;
				double y;
				double z;
				double height;
				double radius;
				if (!double.TryParse(form.X.Text, out x))
				{
					form.X.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(form.Y.Text, out y))
				{
					form.Y.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(form.Z.Text, out z))
				{
					form.Z.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(form.Height.Text, out height) && height>0)
				{
					form.Height.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(form.Radius.Text, out radius) && radius > 0)
				{
					form.Radius.BorderBrush = Brushes.Red;
				}
                else
				{
					var mb = new MeshBuilder();
                    mb.AddCylinder(new Point3D(height / 2.0 - x, y, x),
					new Point3D(height / 2.0 + x, y, x), 2.0 * radius, 180);
					mGeometry = new GeometryModel3D(mb.ToMesh(), new DiffuseMaterial(Brushes.White));
					mGeometry.Transform = new Transform3DGroup();
					group.Children.Add(mGeometry);
					if (!isInitialized)
					{
						center = new Point3D(x, y, z);
						isInitialized = true;
					}
					else
					{
						var direction = new Point3D(x, y, z) - center;
						center += direction / 2.0;
					}
					form.Close();
				}
				
			});
			form.Show();
		}

		private void Cone_button_Click(object sender, RoutedEventArgs e)
		{
			var coneForm = new Primitives.CreateCone();
			coneForm.Ok.Click += new RoutedEventHandler((object s, RoutedEventArgs ea) =>
			{
				double x;
				double y;
				double z;
				double height;
				double top_radius;
				double base_radius;
				if (!double.TryParse(coneForm.X.Text, out x))
				{
					coneForm.X.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(coneForm.Y.Text, out y))
				{
					coneForm.Y.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(coneForm.Z.Text, out z))
				{
					coneForm.Z.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(coneForm.Height.Text, out height))
				{
					coneForm.Height.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(coneForm.TopRadius.Text, out top_radius) || top_radius < 0)
				{
					coneForm.TopRadius.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(coneForm.BaseRadius.Text, out base_radius) || base_radius < 0)
				{
					coneForm.BaseRadius.BorderBrush = Brushes.Red;
				}
				else if (base_radius != 0 || top_radius != 0)
				{
					var mb = new MeshBuilder();
					mb.AddCone(new Point3D(height / 2.0 - x, y, x), new Vector3D(0.0, 0.0, 1.0), base_radius, top_radius,
						height, true, true, 180);
					mGeometry = new GeometryModel3D(mb.ToMesh(), new DiffuseMaterial(Brushes.White));
					bounds.Union(mGeometry.Geometry.Bounds);
					mGeometry.Transform = new Transform3DGroup();
					group.Children.Add(mGeometry);
					if (!isInitialized)
					{
						center = new Point3D(x, y, z);
						isInitialized = true;
					}
					else
					{
						var direction = new Point3D(x, y, z) - center;
						center += direction / 2.0;
					}

					coneForm.Close();
				}
				else
				{
					coneForm.TopRadius.BorderBrush = Brushes.Red;
					coneForm.BaseRadius.BorderBrush = Brushes.Red;
				}
			});
			coneForm.Show();
		}


		private void Cube_button_Click(object sender, RoutedEventArgs e)
		{
			var form = new Primitives.CreateCube();
			form.Ok.Click += new RoutedEventHandler((object s, RoutedEventArgs ea) =>
			{
				double x;
				double y;
				double z;
				double edgeX;
				double edgeY;
				double edgeZ;
				if (!double.TryParse(form.X.Text, out x))
				{
					form.X.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(form.Y.Text, out y))
				{
					form.Y.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(form.Z.Text, out z))
				{
					form.Z.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(form.EdgeX.Text, out edgeX) || edgeX < 0)
				{
					form.EdgeX.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(form.EdgeY.Text, out edgeY) || edgeY < 0)
				{
					form.EdgeY.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(form.EdgeZ.Text, out edgeZ) || edgeZ < 0)
				{
					form.EdgeZ.BorderBrush = Brushes.Red;
				}
				else if (edgeX > 0 && edgeY > 0 && edgeZ > 0)
				{
					var mb = new MeshBuilder();
					mb.AddBox(new Point3D(x, y, z), edgeX, edgeY, edgeZ);
					mGeometry = new GeometryModel3D(mb.ToMesh(), new DiffuseMaterial(Brushes.White));
					bounds.Union(mGeometry.Geometry.Bounds);
					mGeometry.Transform = new Transform3DGroup();
					group.Children.Add(mGeometry);
					if (!isInitialized)
					{
						center = new Point3D(x, y, z);
						isInitialized = true;
					}
					else
					{
						var direction = new Point3D(x, y, z) - center;
						center += direction / 2.0;
					}

					form.Close();
				}
				else
				{
					form.EdgeX.BorderBrush = Brushes.Red;
					form.EdgeY.BorderBrush = Brushes.Red;
					form.EdgeZ.BorderBrush = Brushes.Red;
				}
			});
			form.Show();
		}

		private void Sphere_button_Click(object sender, RoutedEventArgs e)
		{
			var form = new Primitives.CreateSphere();
			form.Ok.Click += new RoutedEventHandler((object s, RoutedEventArgs ea) =>
			{
				double x;
				double y;
				double z;
				double radius;
				if (!double.TryParse(form.X.Text, out x))
				{
					form.X.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(form.Y.Text, out y))
				{
					form.Y.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(form.Z.Text, out z))
				{
					form.Z.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(form.Radius.Text, out radius) || radius <= 0)
				{
					form.Radius.BorderBrush = Brushes.Red;
				}
				else
				{
					var mb = new MeshBuilder();
					mb.AddSphere(new Point3D(x, y, z), radius, 180, 180);
					mGeometry = new GeometryModel3D(mb.ToMesh(), new DiffuseMaterial(Brushes.White));
					bounds.Union(mGeometry.Geometry.Bounds);
					mGeometry.Transform = new Transform3DGroup();
					group.Children.Add(mGeometry);
					if (!isInitialized)
					{
						center = new Point3D(x, y, z);
						isInitialized = true;
					}
					else
					{
						var direction = new Point3D(x, y, z) - center;
						center += direction / 2.0;
					}

					form.Close();
				}
			});
			form.Show();
		}

		private void Pyramid_button_Click(object sender, RoutedEventArgs e)
		{
			var form = new Primitives.CreatePyramid();
			form.Ok.Click += new RoutedEventHandler((object s, RoutedEventArgs ea) =>
			{
				double x;
				double y;
				double z;
				double height;
				double sEdge;
				if (!double.TryParse(form.X.Text, out x))
				{
					form.X.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(form.Y.Text, out y))
				{
					form.Y.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(form.Z.Text, out z))
				{
					form.Z.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(form.SEdge.Text, out sEdge) || sEdge <= 0)
				{
					form.SEdge.BorderBrush = Brushes.Red;
				}
				else if (!double.TryParse(form.Height.Text, out height) || height <= 0)
				{
					form.Height.BorderBrush = Brushes.Red;
				}
				else
				{
					var mb = new MeshBuilder();
					mb.AddPyramid(new Point3D(x, y, z), new Vector3D(1, 0, 0), new Vector3D(0, 0, 1), sEdge, height);
					mGeometry = new GeometryModel3D(mb.ToMesh(), new DiffuseMaterial(Brushes.White));
					bounds.Union(mGeometry.Geometry.Bounds);
					mGeometry.Transform = new Transform3DGroup();
					group.Children.Add(mGeometry);
					if (!isInitialized)
					{
						center = new Point3D(x, y, z);
						isInitialized = true;
					}
					else
					{
						var direction = new Point3D(x, y, z) - center;
						center += direction / 2.0;
					}

					form.Close();
				}
			});
			form.Show();
		}

		private bool CheckAndCreateDatabase(string serverName, string databaseName)
		{
			string sqlCreateDBQuery;
			bool result = false;

			try
			{
				SqlConnection sqlConnection= new SqlConnection("server=" + serverName + ";Trusted_Connection=yes");

				sqlCreateDBQuery = string.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'", databaseName);


				using (sqlConnection)
				{
					sqlConnection.Open();
					using (SqlCommand sqlCmd = new SqlCommand(sqlCreateDBQuery, sqlConnection))
					{
						
						object resultObj = sqlCmd.ExecuteScalar();

						int databaseID = 0;

						if (resultObj != null)
						{
							int.TryParse(resultObj.ToString(), out databaseID);
						}

						
						if (databaseID <= 0)
						{
							var command = sqlConnection.CreateCommand();
							command.CommandText = "CREATE DATABASE "+databaseName;
							command.ExecuteNonQuery();
						}
						sqlConnection.Close();

					}
				}
			}
			catch (Exception ex)
			{
				result = false;
			}

			return result;
		}

		void ClearTable(SqlConnection sqlConnection, string tableName)
		{
			using (var command = new SqlCommand("select case when exists((select * from information_schema.tables where table_name = '" + tableName + "')) then 1 else 0 end", sqlConnection))
			{
				if ((int)command.ExecuteScalar() == 1)
				{
					using (var command1 = new SqlCommand("DROP TABLE IF EXISTS " + tableName, sqlConnection))
					{
						command1.ExecuteNonQuery();
					}

				}
			}
			string string_command = string.Format("CREATE TABLE {0} (ID INT NOT NULL IDENTITY(1,1) PRIMARY KEY, DATA VARBINARY(MAX) )",
						tableName);
			using (var command1 = new SqlCommand(string_command, sqlConnection))
			{
				command1.ExecuteNonQuery();
			}
		}

		public static bool Insert(string data, string tableName, SqlConnection connection)
		{
			string string_command = string.Format("INSERT INTO {0} (Data) VALUES (@Data)",
				tableName);

			var command = new SqlCommand(string_command, connection);
			var param = new SqlParameter("@Data", SqlDbType.Binary)
			{
				Value = ConvertToByteArray(data, Encoding.UTF8)
			};
			command.Parameters.Add(param);
			return command.ExecuteNonQuery() > 0;
		}

		public static byte[] ConvertToByteArray(string str, Encoding encoding)
		{
			return encoding.GetBytes(str);
		}

		private void Save_Button_Click(object sender, RoutedEventArgs e)
		{
			var form = new ConnectDatabaseForm();
			SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
            form.Ok.Click += new RoutedEventHandler((object s, RoutedEventArgs ea) =>
			 {
				 
				 if (form.Auth.IsEnabled)
				 {
					 sqlConnectionStringBuilder.IntegratedSecurity = true;
				 }
				 else
				 {
					 sqlConnectionStringBuilder.UserID = form.UserName.ToString();
					 sqlConnectionStringBuilder.Password = form.Password.Text;
				 }
				 sqlConnectionStringBuilder.DataSource = form.ServerName.Text;
				 sqlConnectionStringBuilder.InitialCatalog = form.DbNameName.Text;
				 try
				 {
					 CheckAndCreateDatabase(sqlConnectionStringBuilder.DataSource, sqlConnectionStringBuilder.InitialCatalog);
					 SqlConnection sqlConnection = new SqlConnection(sqlConnectionStringBuilder.ToString());
					 sqlConnection.Open();
					 if (sqlConnection.State == System.Data.ConnectionState.Open)
					 {
						 form.Close();
						 string tableName = form.TableName.Text;
						 ClearTable(sqlConnection, tableName);
						 foreach (var m in group.Children)
						 {
						 	GeometryModel3D geometryModel3D = m as GeometryModel3D;
						 	if (geometryModel3D == null)
						 		continue;

						 	string savedButton = XamlWriter.Save(geometryModel3D);
							 Insert(savedButton, tableName, sqlConnection);
						 } 
					 }
					 else
					 {
						 var msgResult = MessageBox.Show("Parameters of connection is invalid");
					 }
				 }
				 catch (Exception ex)
				 {
					 var msgResult = MessageBox.Show(ex.Message, "Error");
				 }
			 });
			form.Show();
			
			
			
				 //int i = 0;
				 //IFormatter formatter = new BinaryFormatter();
				 //foreach (var m in group.Children)
				 //{
				 //	GeometryModel3D geometryModel3D = m as GeometryModel3D;
				 //	if (geometryModel3D == null)
				 //		continue;

				 //	string savedButton = XamlWriter.Save(geometryModel3D);
				 //	File.WriteAllText("E:\\test\\Geom" + i + ".bin", savedButton, Encoding.UTF8);
				 //             ++i;
				 //}
				 //StreamReader streamReader = new StreamReader("E:\\test\\Geom0.bin");
			//XmlReader xmlReader = XmlReader.Create(streamReader);
			//GeometryModel3D readerLoadButton = (GeometryModel3D)XamlReader.Load(xmlReader);
			//group.Children.Add(readerLoadButton);
		}
		
		private void Read_Button_Click(object sender, RoutedEventArgs e)
		{
			var form = new ConnectDatabaseForm();
			SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
			form.Ok.Click += new RoutedEventHandler((object s, RoutedEventArgs ea) =>
			{

				if (form.Auth.IsEnabled)
				{
					sqlConnectionStringBuilder.IntegratedSecurity = true;
				}
				else
				{
					sqlConnectionStringBuilder.UserID = form.UserName.ToString();
					sqlConnectionStringBuilder.Password = form.Password.Text;
				}
				sqlConnectionStringBuilder.DataSource = form.ServerName.Text;
				sqlConnectionStringBuilder.InitialCatalog = form.DbNameName.Text;
				try
				{
					SqlConnection sqlConnection = new SqlConnection(sqlConnectionStringBuilder.ToString());
					sqlConnection.Open();
					if (sqlConnection.State == System.Data.ConnectionState.Open)
					{
						form.Close();
						string tableName = form.TableName.Text;
						using (var command = new SqlCommand("SELECT * FROM " + tableName, sqlConnection))
						{
							var reader = command.ExecuteReader();
							while (reader.Read())
							{
								var v = reader.GetSqlBinary(1);
								var str = System.Text.Encoding.UTF8.GetString(reader.GetSqlBinary(1).Value);
								StringReader streamReader = new StringReader(str);
								XmlReader xmlReader = XmlReader.Create(streamReader);
								GeometryModel3D geometry = (GeometryModel3D)XamlReader.Load(xmlReader);
								group.Children.Add(geometry);
							}
						}
					}
					else
					{
						var msgResult = MessageBox.Show("Parameters of connection is invalid");
					}
				}
				catch (Exception ex)
				{
					var msgResult = MessageBox.Show(ex.Message, "Error");
				}
			});
			form.Show();



			//int i = 0;
			//IFormatter formatter = new BinaryFormatter();
			//foreach (var m in group.Children)
			//{
			//	GeometryModel3D geometryModel3D = m as GeometryModel3D;
			//	if (geometryModel3D == null)
			//		continue;

			//	string savedButton = XamlWriter.Save(geometryModel3D);
			//	File.WriteAllText("E:\\test\\Geom" + i + ".bin", savedButton, Encoding.UTF8);
			//             ++i;
			//}
			//StreamReader streamReader = new StreamReader("E:\\test\\Geom0.bin");
			//XmlReader xmlReader = XmlReader.Create(streamReader);
			//GeometryModel3D readerLoadButton = (GeometryModel3D)XamlReader.Load(xmlReader);
			//group.Children.Add(readerLoadButton);
		}
	}

}
