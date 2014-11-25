using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZedGraph;
using System.Drawing.Imaging;
using System.Drawing;

namespace XBRLReportBuilder
{
	public partial class ReportBuilder
	{
		/// <summary>
		/// Creates an image of a barchart which represents the data in <paramref name="barChartData"/>.  The image is saved at <paramref name="completeFileName"/>.
		/// </summary>
		/// <param name="completeFileName">The path at which to save the generated image.</param>
		/// <param name="barChartData">The &lt;year, value&gt; plots for the barchart.</param>
		/// <returns>True on success, false on fail.</returns>
		private bool CreateBarChartImageFile( string completeFileName, SortedDictionary<int, double> barChartData )
		{
			if( Thread.CurrentThread.GetApartmentState() != ApartmentState.STA )
			{
				return this.InvokeCreateBarChartImageFile( completeFileName, barChartData );
			}

			try
			{
				const string fontFamily = "Times New Roman";
				const float fontSize = 12f;


				ZedGraphControl chart = new ZedGraphControl();
				chart.Size = new Size( ( barChartData.Count + 2 ) * 45, 300 );

				GraphPane pane = chart.GraphPane;
				pane.IsFontsScaled = false;

				pane.BarSettings.Type = BarType.Stack;
				pane.Chart.Fill = new Fill( Color.Transparent );
				pane.Fill = new Fill( Color.LightBlue, Color.AliceBlue, 90F );

				pane.Title.FontSpec.Family = fontFamily;
				pane.Title.FontSpec.Size = fontSize;
				pane.Title.IsVisible = false;
				pane.Title.Text = string.Empty;

				pane.XAxis.MajorGrid.IsVisible = false;

				pane.XAxis.MajorTic.IsInside = false;

				pane.XAxis.Scale.FontSpec.Family = fontFamily;
				pane.XAxis.Scale.FontSpec.Size = fontSize;

				pane.XAxis.Title.FontSpec.Family = fontFamily;
				pane.XAxis.Title.FontSpec.Size = fontSize;
				pane.XAxis.Title.IsVisible = false;
				pane.XAxis.Title.Text = string.Empty;

				pane.X2Axis.IsVisible = false;
				pane.X2Axis.MajorGrid.IsVisible = false;
				pane.X2Axis.MinorGrid.IsVisible = false;
				pane.X2Axis.MajorTic.IsInside = false;
				pane.X2Axis.MajorTic.IsOutside = false;
				pane.X2Axis.MinorTic.IsInside = false;
				pane.X2Axis.MinorTic.IsOutside = false;

				pane.YAxis.MajorGrid.IsVisible = true;
				pane.YAxis.MajorGrid.Color = Color.LightSteelBlue;


				pane.YAxis.MajorTic.IsInside = false;

				pane.YAxis.MinorTic.IsInside = false;
				pane.YAxis.MinorTic.IsOutside = false;


				pane.YAxis.Scale.FontSpec.Family = fontFamily;
				pane.YAxis.Scale.FontSpec.Size = fontSize;

				pane.YAxis.Title.FontSpec.Family = fontFamily;
				pane.YAxis.Title.FontSpec.Size = fontSize;
				pane.YAxis.Title.IsVisible = false;
				pane.YAxis.Title.Text = string.Empty;


				pane.Y2Axis.IsVisible = false;
				pane.Y2Axis.MajorGrid.IsVisible = false;
				pane.Y2Axis.MinorGrid.IsVisible = false;
				pane.Y2Axis.MajorTic.IsInside = false;
				pane.Y2Axis.MajorTic.IsOutside = false;
				pane.Y2Axis.MinorTic.IsInside = false;
				pane.Y2Axis.MinorTic.IsOutside = false;

				int idx = 0;
				string[] labels = new string[ barChartData.Count ];

				PointPairList nPoints = new PointPairList();
				PointPairList pPoints = new PointPairList();
				foreach( KeyValuePair<int, double> yearValue in barChartData )
				{
					int yr = yearValue.Key % 100;
					labels[ idx ] = "'" + yr.ToString( "00" );

					double value = yearValue.Value * 100;
					if( yearValue.Value >= 0 )
					{
						nPoints.Add( yearValue.Key, 0.0 );
						pPoints.Add( yearValue.Key, value );
					}
					else
					{
						nPoints.Add( yearValue.Key, value );
						pPoints.Add( yearValue.Key, 0.0 );
					}

					idx++;
				}

				pane.XAxis.Scale.TextLabels = labels;
				pane.XAxis.Type = AxisType.Text;


				//negative values
				BarItem nBar = pane.AddBar( string.Empty, nPoints, Color.Empty );
				nBar.Bar.Fill = new Fill( Color.FromArgb( 227, 99, 52 ), Color.FromArgb( 255, 177, 126 ), Color.FromArgb( 227, 99, 52 ) );

				//positive values
				BarItem pBar = pane.AddBar( string.Empty, pPoints, Color.Empty );
				pBar.Bar.Fill = new Fill( Color.FromArgb( 81, 148, 157 ), Color.FromArgb( 107, 197, 222 ), Color.FromArgb( 81, 148, 157 ) );

				//1 - Update the chart with the series
				chart.AxisChange();

				//2 - Process the value labels for each point
				this.WriteValueLabels( pane, nBar, fontFamily, fontSize, true );
				this.WriteValueLabels( pane, pBar, fontFamily, fontSize, false );

				//3 - Update the chart again
				chart.AxisChange();

				using( Image img = chart.GetImage() )
				{
					EncoderParameters parms = new EncoderParameters( 1 );
					EncoderParameter parm = new EncoderParameter( System.Drawing.Imaging.Encoder.Quality, 80L );
					parms.Param[ 0 ] = parm;

					ImageCodecInfo jgpEncoder = GetEncoder( ImageFormat.Jpeg );
					img.Save( completeFileName, jgpEncoder, parms );
				}

				try
				{
					chart.Dispose();
					chart = null;
				}
				catch { }

				return true;
			}
			catch { }

			return false;
		}

		private ImageCodecInfo GetEncoder( ImageFormat format )
		{
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
			foreach( ImageCodecInfo codec in codecs )
			{
				if( codec.FormatID == format.Guid )
					return codec;
			}
			return null;
		}

		private void WriteValueLabels( GraphPane pane, BarItem bar,
			string fontFamily, float fontSize, bool isNegative )
		{
			//shrink the bar font just a little
			fontSize -= 1;

			double xMin, xMax;
			double yMin, yMax;
			bar.GetRange( out xMin, out xMax, out yMin, out yMax, false, false, pane );

			//adjust singular values so that the label doesn't bleed into the bar
			if( isNegative )
				yMax = Math.Max( yMax, 0 );
			else
				yMin = Math.Min( yMin, 0 );

			double difference = Math.Abs( yMax - yMin );
			double offset = bar.Points.Count == 1 ?
				0.05 * difference :
				0.1 * difference;

			for( int i = 0; i < bar.Points.Count; i++ )
			{
				PointPair pt = bar.Points[ i ];
				if( pt.Y == 0.0 )
					continue;

				double myOffset = pt.Y > 0 ? offset : -offset;
				double myShift = pt.Y > 0 ? 0.75 : 0.65;

				string label = pt.Y.ToString( "f2" ) + "%";

				// Create a text label from the Y data value
				TextObj text = new TextObj( label, ( i + myShift ), pt.Y + myOffset,
					CoordType.AxisXYScale, AlignH.Left, AlignV.Center );

				text.FontSpec.Family = fontFamily;
				text.FontSpec.Size = fontSize;

				text.FontSpec.Angle = 0;
				text.FontSpec.Border.IsVisible = false;
				text.FontSpec.Fill.IsVisible = false;

				text.Location.AlignH = AlignH.Left;
				text.Location.AlignV = AlignV.Center;
				text.Location.CoordinateFrame = CoordType.AxisXYScale;

				text.ZOrder = ZOrder.A_InFront;

				pane.GraphObjList.Add( text );
			}
		}

		private bool InvokeCreateBarChartImageFile( string completeFileName, SortedDictionary<int, double> barChartData )
		{
			ParameterizedThreadStart pts = new ParameterizedThreadStart(
				( obj ) => { this.CreateBarChartImageFile( completeFileName, barChartData ); }
			);

			try
			{
				Thread t = new Thread( pts );
				t.SetApartmentState( ApartmentState.STA );
				t.Start();
				t.Join();

				return true;
			}
			catch { }

			return false;
		}

		private static double GetRangeModifier( double value, int factor )
		{
			double modifier = value % factor;

			if( modifier < 0 )
				modifier += factor;
			else
				modifier = factor - modifier;

			modifier += factor;
			return modifier;
		}
	}
}
