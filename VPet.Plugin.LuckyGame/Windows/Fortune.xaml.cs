using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using VPet.Plugin.LuckyGame.Core;
using VPet.Plugin.LuckyGame.Core.Game;

namespace VPet.Plugin.LuckyGame.Windows
{
    public partial class Fortune : Window
    {
        private LuckyWheel luckyWheel;
        private DispatcherTimer animationTimer;
        private RotateTransform wheelTransform;
        private bool isSpinning = false;
        private bool isInitialised = false;
        private Point startPoint;
        private bool isDragging = false;
        private ulong coin = 1;
        private ushort place = 1, allPlace = 6;
        private bool UIInitialized = false;
        private readonly GameTokenCoin gtc;
        // 转盘配置
        private int sectorCount => prizes.Count;

        private List<string> prizes = [];
        private Color[] sectorColors = {
            Color.FromRgb(255, 99, 99),    // 红色
            Color.FromRgb(255, 177, 66),   // 橙色
            Color.FromRgb(255, 222, 89),  // 黄色
            Color.FromRgb(123, 220, 181), // 绿色
            Color.FromRgb(87, 160, 211),  // 蓝色
            Color.FromRgb(152, 117, 250), // 紫色
            Color.FromRgb(255, 138, 216), // 粉色
            Color.FromRgb(128, 203, 196)  // 青色
        };

        // 设置项属性
        public int TokenCost { get => Convert.ToInt32(coin); private set { coin = Convert.ToUInt64(value); } }
        public int SectorCount { get => Convert.ToInt32(allPlace); private set { allPlace = Convert.ToUInt16(value); } } 
        public int PredictionPoints { get => Convert.ToInt32(place); private set { place = Convert.ToUInt16(value); } }

        internal Fortune(GameTokenCoin gtc)
        {
            this.gtc = gtc;
            InitializeComponent();
            UIInitialized = true;
            InitializeGame();
            LoadSettings();
            InitializeWheelUI();
        }

        private void InitializeGame()
        {
            // 初始化转盘算法
            luckyWheel = new LuckyWheel();
            luckyWheel.OnAngelChange = OnWheelAngleChanged;

            // 初始化动画计时器
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / 60); // 60FPS
            animationTimer.Tick += OnAnimationFrame;

            // 初始化奖品列表
            InitializePrizes();
        }

        private void InitializePrizes()
        {
            // 根据转盘格数初始化奖品
            prizes.Clear();
            for (int i = 1; i <= SectorCount; i++)
            {
                prizes.Add($"{i}");
            }
        }

        private void InitializeWheelUI()
        { 
            // 创建旋转变换
            wheelTransform = new RotateTransform();
            WheelCanvas.RenderTransform = wheelTransform;
            WheelCanvas.RenderTransformOrigin = new Point(0.5, 0.5);

            // 绘制转盘
            DrawWheelSectors();
            isInitialised = true;
        }

        #region 绘图部分
        private void DrawWheelSectors()
        {
            WheelCanvas.Children.Clear();

            double centerX = 200; // 使用固定中心点
            double centerY = 200;
            double radius = 150;
            double anglePerSector = 360.0 / sectorCount;

            // 添加转盘底座
            AddWheelBase(centerX, centerY, radius);

            for (int i = 0; i < sectorCount; i++)
            {
                double startAngle = i * anglePerSector;
                double endAngle = (i + 1) * anglePerSector;

                // 创建扇形
                Path sector = CreateSector(centerX, centerY, radius, startAngle, endAngle, sectorColors[i % sectorColors.Length]);
                WheelCanvas.Children.Add(sector);

                // 创建扇形标签
                TextBlock label = CreateSectorLabel(centerX, centerY, radius * 0.6,
                    startAngle + anglePerSector / 2, prizes[i]);
                WheelCanvas.Children.Add(label);
            }

            // 添加中心圆
            AddCenter(centerX, centerY);
        }

        private void AddWheelBase(double centerX, double centerY, double radius)
        {
            // 转盘外圈
            Ellipse outerCircle = new Ellipse
            {
                Width = radius * 2 + 20,
                Height = radius * 2 + 20,
                Stroke = Brushes.Gold,
                StrokeThickness = 3,
                Fill = Brushes.Transparent
            };
            Canvas.SetLeft(outerCircle, centerX - radius - 10);
            Canvas.SetTop(outerCircle, centerY - radius - 10);
            WheelCanvas.Children.Add(outerCircle);

            // 转盘底座
            Ellipse baseCircle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = new SolidColorBrush(Color.FromArgb(100, 50, 50, 50)),
                Stroke = Brushes.Gray,
                StrokeThickness = 2
            };
            Canvas.SetLeft(baseCircle, centerX - radius);
            Canvas.SetTop(baseCircle, centerY - radius);
            WheelCanvas.Children.Add(baseCircle);
        }

        private Path CreateSector(double centerX, double centerY, double radius, double startAngle, double endAngle, Color color)
        {
            try
            {
                PathFigure pathFigure = new PathFigure();

                // 起始点（圆心）
                pathFigure.StartPoint = new Point(centerX, centerY);

                // 计算弧线点
                double startRad = startAngle * Math.PI / 180;
                double endRad = endAngle * Math.PI / 180;

                Point startPoint = new Point(
                    centerX + radius * Math.Sin(startRad),
                    centerY - radius * Math.Cos(startRad));

                Point endPoint = new Point(
                    centerX + radius * Math.Sin(endRad),
                    centerY - radius * Math.Cos(endRad));

                pathFigure.Segments.Add(new LineSegment(startPoint, true));

                // 添加弧线段
                bool isLargeArc = (endAngle - startAngle) > 180;
                ArcSegment arc = new ArcSegment(
                    endPoint,
                    new Size(radius, radius),
                    0, isLargeArc, SweepDirection.Clockwise, true);

                pathFigure.Segments.Add(arc);
                pathFigure.Segments.Add(new LineSegment(new Point(centerX, centerY), true));

                PathGeometry pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(pathFigure);

                return new Path
                {
                    Data = pathGeometry,
                    Fill = new SolidColorBrush(color),
                    Stroke = Brushes.White,
                    StrokeThickness = 1,
                    Opacity = 0.8
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建扇形失败: {ex.Message}");
                return new Path();
            }
        }

        private TextBlock CreateSectorLabel(double centerX, double centerY, double labelRadius, double angle, string text)
        {
            try
            {
                double angleRad = angle * Math.PI / 180;
                double x = centerX + labelRadius * Math.Sin(angleRad);
                double y = centerY - labelRadius * Math.Cos(angleRad);

                TextBlock label = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    FontSize = 10,
                    Width = 50,
                    Height = 20,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    RenderTransform = new RotateTransform(angle + 90, 25, 10)
                };

                Canvas.SetLeft(label, x - 25);
                Canvas.SetTop(label, y - 10);

                return label;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建标签失败: {ex.Message}");
                return new TextBlock();
            }
        }

        private void AddCenter(double centerX, double centerY)
        {
            // 中心圆装饰
            Ellipse centerDecor = new Ellipse
            {
                Width = 40,
                Height = 40,
                Fill = Brushes.Gold,
                Stroke = Brushes.DarkGoldenrod,
                StrokeThickness = 2
            };
            Canvas.SetLeft(centerDecor, centerX - 20);
            Canvas.SetTop(centerDecor, centerY - 20);
            WheelCanvas.Children.Add(centerDecor);

            // 中心圆
            Ellipse centerCircle = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.Red,
                Stroke = Brushes.DarkRed,
                StrokeThickness = 2
            };
            Canvas.SetLeft(centerCircle, centerX - 10);
            Canvas.SetTop(centerCircle, centerY - 10);
            WheelCanvas.Children.Add(centerCircle);
        }

        private void OnWheelAngleChanged(float angle)
        {
            // 在UI线程上更新角度
            Dispatcher.Invoke(() =>
            {
                if (wheelTransform != null)
                {
                    wheelTransform.Angle = angle;
                }
            });
        }

        private void OnAnimationFrame(object sender, EventArgs e)
        {
            // 可以在这里添加额外的动画效果
        }
        #endregion

        private async void SpinButton_Click(object sender, RoutedEventArgs e)
        {
            if (isSpinning) return;
            if (!isInitialised)
            {
                MessageBoxX.Show("转盘未初始化，请点击重置后继续".Translate(), "警告".Translate());
                return;
            }

            isSpinning = true;
            SpinButton.IsEnabled = false;
            SpinButton.Content = "旋转中...".Translate();

            try
            {
                // 开始转盘旋转
                var game = new LuckyWheel.LuckyWheelBuy
                {
                    Coin = coin,
                    Place = place,
                    AllPlace = allPlace,
                    CoinType = gtc.defCoinType
                };
                var buyresult = luckyWheel.PlaceCoin(game, gtc);
                switch (buyresult)
                {
                    case 0:break;
                    case 1:
                        MessageBoxX.Show("出现未知错误，无法进行抽奖".Translate(), "错误".Translate());
                        return;
                    case 2:
                        MessageBoxX.Show("押注代币为0，无法进行抽奖".Translate(), "错误".Translate());
                        return;
                    case 3:
                        MessageBoxX.Show("代币余额不足，无法进行抽奖".Translate(), "错误".Translate());
                        return;
                }
                var result = await luckyWheel.StartWheel(game);

                ShowResult(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"转盘旋转出错: {ex.Message}".Translate(), "错误".Translate(),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isSpinning = false;
                SpinButton.IsEnabled = true;
                SpinButton.Content = "开始旋转".Translate();
            }
        }

        private ushort CalculateResult(float finalAngle)
        {
            double anglePerSector = 360.0 / sectorCount;
            // 计算落在哪个扇形（考虑指针在顶部）
            ushort resultIndex = Convert.ToUInt16((int)((360 - finalAngle % 360) / anglePerSector) % sectorCount);
            return resultIndex;
        }

        private void ShowResult(LuckyWheel.LuckyWheelResult result)
        {
            try
            {
                var resultIndex = CalculateResult(result.StopAngle);
                var prize = LuckyWheel.WinCoin(resultIndex,result,gtc);

                MessageBoxX.Show("恭喜您获得: {0} 代币!".Translate(prize), "抽奖结果".Translate());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"显示结果时出错: {ex.Message}".Translate(), "错误".Translate(),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSpinning)
            {
                wheelTransform.Angle = 0;
                isInitialised = true;
            }
        }

        private void LoadSettings()
        {
            try
            {
                // 从配置文件加载设置
                TokenCostText.Value = TokenCost;
                PredictionPointsText.Value = PredictionPoints;

                // 设置ComboBox选中项
                foreach (ComboBoxItem item in SectorCountComboBox.Items)
                {
                    if (item.Tag != null && int.Parse(item.Tag.ToString()) == SectorCount)
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex.Message}");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            animationTimer?.Stop();
        }

        private void WheelCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 当Canvas大小改变时重绘转盘
            if (e.NewSize.Width > 0 && e.NewSize.Height > 0 && isInitialised)
            {
                DrawWheelSectors();
            }
        }

        private void ExitButton_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }

        #region 窗口拖动
        private void DragArea_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = true;
                startPoint = e.GetPosition(this);
                ((UIElement)sender).CaptureMouse();
            }
        }

        private void DragArea_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(this);

                // 计算移动距离
                double deltaX = currentPoint.X - startPoint.X;
                double deltaY = currentPoint.Y - startPoint.Y;

                // 移动窗口
                this.Left += deltaX;
                this.Top += deltaY;
            }
        }

        private void DragArea_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isDragging = false;
            ((UIElement)sender).ReleaseMouseCapture();
        }
        #endregion

        private void TokenCostText_ValueChanged(object sender, Panuon.WPF.SelectedValueChangedRoutedEventArgs<double?> e)
        {
            if (!UIInitialized) return;
            TokenCost = TokenCostText.Value.HasValue ? Convert.ToInt32(TokenCostText.Value.Value) : 1;
        }

        private void SectorCountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!UIInitialized) return;
            if (SectorCountComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                SectorCount = selectedItem.Tag != null ? int.Parse(selectedItem.Tag.ToString()) : 8;
                PredictionPointsText.Maximum = SectorCount;
                InitializePrizes();
                DrawWheelSectors();
            }
        }
    }
}