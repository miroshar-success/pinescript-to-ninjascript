#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class AverageTrueRangeStopLossFinder : Indicator
	{
		private Series<double> source1;
		private Series<double> source2;
		private Series<double> tr;
		private double a;
		private double x;
		private double x2;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "AverageTrueRangeStopLossFinder";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				length 							= 14;
				smoothing 						= MAtypeForATRSLF.RMA;
				m								= 1.5;
				src1							= SourceTypeForATRSLF.HIGH;
				src2							= SourceTypeForATRSLF.LOW;
				pline							= true;
				col1							= Brushes.Blue;
				col2							= Brushes.Teal;
				col3							= Brushes.Red;
				collong							= Brushes.Teal;
				colshort						= Brushes.Red;
				
				AddPlot(new Stroke(colshort, DashStyleHelper.Solid, 1, 80), PlotStyle.Line, "X");
				AddPlot(new Stroke(collong, DashStyleHelper.Solid, 1, 80), PlotStyle.Line, "X2");
			}
			else if (State == State.Configure)
			{
				source1 = new Series<double>(this);
				source2 = new Series<double>(this);
				tr = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			switch(src1)
			{
				case SourceTypeForATRSLF.CLOSE:
					source1[0]	= Close[0];
					break;
				case SourceTypeForATRSLF.HIGH:
					source1[0]	= High[0];
					break;
				case SourceTypeForATRSLF.HL2:
					source1[0]	= (High[0] + Close[0])/2;					
					break;
				case SourceTypeForATRSLF.HLC3:
					source1[0]	= (High[0] + Close[0] + Low[0])/3;
					break;
				case SourceTypeForATRSLF.HLCC4:
					source1[0]	= (High[0] + Close[0] + Close[0] + Low[0])/4;
					break;
				case SourceTypeForATRSLF.OHLC4:
					source1[0]	= (High[0] + Open[0] + Close[0] + Low[0])/4;
					break;
				case SourceTypeForATRSLF.LOW:
					source1[0]	= Low[0];
					break;
				case SourceTypeForATRSLF.OPEN:
					source1[0]	= Open[0];
					break;
			}
			
			switch(src2)
			{
				case SourceTypeForATRSLF.CLOSE:
					source2[0]	= Close[0];
					break;
				case SourceTypeForATRSLF.HIGH:
					source2[0]	= High[0];
					break;
				case SourceTypeForATRSLF.HL2:
					source2[0]	= (High[0] + Close[0])/2;					
					break;
				case SourceTypeForATRSLF.HLC3:
					source2[0]	= (High[0] + Close[0] + Low[0])/3;
					break;
				case SourceTypeForATRSLF.HLCC4:
					source2[0]	= (High[0] + Close[0] + Close[0] + Low[0])/4;
					break;
				case SourceTypeForATRSLF.OHLC4:
					source2[0]	= (High[0] + Open[0] + Close[0] + Low[0])/4;
					break;
				case SourceTypeForATRSLF.LOW:
					source2[0]	= Low[0];
					break;
				case SourceTypeForATRSLF.OPEN:
					source2[0]	= Open[0];
					break;
			}
			
			tr[0] = CurrentBar == 0 ? 0 : Math.Max(High[0] - Low[0], Math.Max(Math.Abs(High[0] - Close[1]), Math.Abs(Low[0] - Close[1])));
			
			if (CurrentBar < length) return;
			
			a = ma_function(tr, length) * m;
			x = ma_function(tr, length) * m + source1[0];
			x2 = source2[0] - ma_function(tr, length) * m;
			X[0] = x;
			X2[0] = x2;
			
		}
		
		private double ma_function(Series<double> src1, int length1)
		{
			double ma = 0;
			switch(smoothing) 
			{
				case MAtypeForATRSLF.SMA:
			        ma = SMA(src1, length1)[0];
				    break;
				case MAtypeForATRSLF.EMA:
				    ma = EMA(src1, length1)[0];
					break;
				case MAtypeForATRSLF.WMA:
			        ma = WMA(src1, length1)[0];
					break;
				case MAtypeForATRSLF.RMA:
				    ma = RMA(src1, length1)[0];
				    break;
			}
			return ma;
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);			
			
			SharpDX.Direct2D1.Brush color;
			color = Brushes.White.ToDxBrush(RenderTarget);

			NinjaTrader.Gui.Tools.SimpleFont simpleFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", 16);
			SharpDX.DirectWrite.TextFormat textFormat = simpleFont.ToDirectWriteTextFormat();
			
			string msg = "ATR: " + Math.Round(a, 1) + ", H: " + Math.Round(x, 1) + ", L: " + Math.Round(x2, 1);
			
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, msg, textFormat, 400, textFormat.FontSize);
			SharpDX.Vector2 lowerTextPoint = new SharpDX.Vector2(ChartPanel.W / 2-textLayout.Metrics.Width / 2, ChartPanel.H-textLayout.Metrics.Height);
			RenderTarget.DrawTextLayout(lowerTextPoint, textLayout, color, SharpDX.Direct2D1.DrawTextOptions.NoSnap);

			color.Dispose();
			textFormat.Dispose();
			textLayout.Dispose();
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Length", Order=1, GroupName="Parameters")]
		public int length
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name= "Smoothing", GroupName = "Parameters", Order = 2)]
		public MAtypeForATRSLF smoothing
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Multiplier", Order=2, GroupName="Parameters")]
		public double m
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name = "Source Type1", GroupName = "Parameters", Order = 3)]
		public SourceTypeForATRSLF src1
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name = "Source Type2", GroupName = "Parameters", Order = 3)]
		public SourceTypeForATRSLF src2
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowPriceLines", Order=3, GroupName="Parameters")]
		public bool pline
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="ATRTextColor", Order=4, GroupName="Parameters")]
		public Brush col1
		{ get; set; }

		[Browsable(false)]
		public string col1Serializable
		{
			get { return Serialize.BrushToString(col1); }
			set { col1 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="LowTextColor", Order=5, GroupName="Parameters")]
		public Brush col2
		{ get; set; }

		[Browsable(false)]
		public string col2Serializable
		{
			get { return Serialize.BrushToString(col2); }
			set { col2 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="HighTextColor", Order=6, GroupName="Parameters")]
		public Brush col3
		{ get; set; }

		[Browsable(false)]
		public string col3Serializable
		{
			get { return Serialize.BrushToString(col3); }
			set { col3 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="LowLineColor", Order=7, GroupName="Parameters")]
		public Brush collong
		{ get; set; }

		[Browsable(false)]
		public string collongSerializable
		{
			get { return Serialize.BrushToString(collong); }
			set { collong = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="HighLineColor", Order=8, GroupName="Parameters")]
		public Brush colshort
		{ get; set; }

		[Browsable(false)]
		public string colshortSerializable
		{
			get { return Serialize.BrushToString(colshort); }
			set { colshort = Serialize.StringToBrush(value); }
		}			

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> X
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> X2
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

public enum MAtypeForATRSLF
{
	RMA,
	SMA,
	EMA,
	WMA	
}

public enum SourceTypeForATRSLF
{
	CLOSE,
	OPEN,
	HIGH,
	LOW,
	HL2,
	HLC3,
	HLCC4,
	OHLC4
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AverageTrueRangeStopLossFinder[] cacheAverageTrueRangeStopLossFinder;
		public AverageTrueRangeStopLossFinder AverageTrueRangeStopLossFinder(int length, MAtypeForATRSLF smoothing, double m, SourceTypeForATRSLF src1, SourceTypeForATRSLF src2, bool pline, Brush col1, Brush col2, Brush col3, Brush collong, Brush colshort)
		{
			return AverageTrueRangeStopLossFinder(Input, length, smoothing, m, src1, src2, pline, col1, col2, col3, collong, colshort);
		}

		public AverageTrueRangeStopLossFinder AverageTrueRangeStopLossFinder(ISeries<double> input, int length, MAtypeForATRSLF smoothing, double m, SourceTypeForATRSLF src1, SourceTypeForATRSLF src2, bool pline, Brush col1, Brush col2, Brush col3, Brush collong, Brush colshort)
		{
			if (cacheAverageTrueRangeStopLossFinder != null)
				for (int idx = 0; idx < cacheAverageTrueRangeStopLossFinder.Length; idx++)
					if (cacheAverageTrueRangeStopLossFinder[idx] != null && cacheAverageTrueRangeStopLossFinder[idx].length == length && cacheAverageTrueRangeStopLossFinder[idx].smoothing == smoothing && cacheAverageTrueRangeStopLossFinder[idx].m == m && cacheAverageTrueRangeStopLossFinder[idx].src1 == src1 && cacheAverageTrueRangeStopLossFinder[idx].src2 == src2 && cacheAverageTrueRangeStopLossFinder[idx].pline == pline && cacheAverageTrueRangeStopLossFinder[idx].col1 == col1 && cacheAverageTrueRangeStopLossFinder[idx].col2 == col2 && cacheAverageTrueRangeStopLossFinder[idx].col3 == col3 && cacheAverageTrueRangeStopLossFinder[idx].collong == collong && cacheAverageTrueRangeStopLossFinder[idx].colshort == colshort && cacheAverageTrueRangeStopLossFinder[idx].EqualsInput(input))
						return cacheAverageTrueRangeStopLossFinder[idx];
			return CacheIndicator<AverageTrueRangeStopLossFinder>(new AverageTrueRangeStopLossFinder(){ length = length, smoothing = smoothing, m = m, src1 = src1, src2 = src2, pline = pline, col1 = col1, col2 = col2, col3 = col3, collong = collong, colshort = colshort }, input, ref cacheAverageTrueRangeStopLossFinder);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AverageTrueRangeStopLossFinder AverageTrueRangeStopLossFinder(int length, MAtypeForATRSLF smoothing, double m, SourceTypeForATRSLF src1, SourceTypeForATRSLF src2, bool pline, Brush col1, Brush col2, Brush col3, Brush collong, Brush colshort)
		{
			return indicator.AverageTrueRangeStopLossFinder(Input, length, smoothing, m, src1, src2, pline, col1, col2, col3, collong, colshort);
		}

		public Indicators.AverageTrueRangeStopLossFinder AverageTrueRangeStopLossFinder(ISeries<double> input , int length, MAtypeForATRSLF smoothing, double m, SourceTypeForATRSLF src1, SourceTypeForATRSLF src2, bool pline, Brush col1, Brush col2, Brush col3, Brush collong, Brush colshort)
		{
			return indicator.AverageTrueRangeStopLossFinder(input, length, smoothing, m, src1, src2, pline, col1, col2, col3, collong, colshort);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AverageTrueRangeStopLossFinder AverageTrueRangeStopLossFinder(int length, MAtypeForATRSLF smoothing, double m, SourceTypeForATRSLF src1, SourceTypeForATRSLF src2, bool pline, Brush col1, Brush col2, Brush col3, Brush collong, Brush colshort)
		{
			return indicator.AverageTrueRangeStopLossFinder(Input, length, smoothing, m, src1, src2, pline, col1, col2, col3, collong, colshort);
		}

		public Indicators.AverageTrueRangeStopLossFinder AverageTrueRangeStopLossFinder(ISeries<double> input , int length, MAtypeForATRSLF smoothing, double m, SourceTypeForATRSLF src1, SourceTypeForATRSLF src2, bool pline, Brush col1, Brush col2, Brush col3, Brush collong, Brush colshort)
		{
			return indicator.AverageTrueRangeStopLossFinder(input, length, smoothing, m, src1, src2, pline, col1, col2, col3, collong, colshort);
		}
	}
}

#endregion
