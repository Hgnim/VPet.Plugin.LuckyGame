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
        private ulong coin;
        private ushort place, allPlace;

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

        internal Fortune(GameTokenCoin gtc)
        {
            InitializeComponent();
            InitializeGame();
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

            // 初始化转盘UI
            InitializeWheelUI();
        }

        private void InitializeWheelUI()
        {
            WheelBorder.Visibility = Visibility.Visible;
            ControlPanel.Visibility = Visibility.Visible;
            SettingBorder.Visibility = Visibility.Collapsed;

            luckyWheel.PlaceCoin(coin, place, allPlace);
            // 创建旋转变换
            wheelTransform = new RotateTransform();
            WheelCanvas.RenderTransform = wheelTransform;
            WheelCanvas.RenderTransformOrigin = new Point(0.5, 0.5);
            
            DrawWheelSectors();
            isInitialised = true;
        }

        private void DrawWheelSectors()
        {
            double centerX = WheelCanvas.ActualWidth / 2;
            double centerY = WheelCanvas.ActualHeight / 2;
            double radius = Math.Min(centerX, centerY) - 20;
            double anglePerSector = 360.0 / sectorCount;

            for (int i = 0; i < sectorCount; i++)
            {
                double startAngle = i * anglePerSector;
                double endAngle = (i + 1) * anglePerSector;

                // 创建扇形
                Path sector = CreateSector(centerX, centerY, radius, startAngle, endAngle, sectorColors[i%8]);
                WheelCanvas.Children.Add(sector);

                // 创建扇形标签
                TextBlock label = CreateSectorLabel(centerX, centerY, radius * 0.6,
                    startAngle + anglePerSector / 2, prizes[i]);
                WheelCanvas.Children.Add(label);
            }

            // 添加中心圆
            AddCenter(centerX, centerY);
        }

        private Path CreateSector(double centerX, double centerY, double radius, double startAngle, double endAngle, Color color)
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
                StrokeThickness = 2
            };
        }

        private TextBlock CreateSectorLabel(double centerX, double centerY, double labelRadius, double angle, string text)
        {
            double angleRad = angle * Math.PI / 180;
            double x = centerX + labelRadius * Math.Sin(angleRad);
            double y = centerY - labelRadius * Math.Cos(angleRad);

            TextBlock label = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Width = 60,
                Height = 20,
                TextAlignment = TextAlignment.Center,
                RenderTransform = new RotateTransform(angle + 90, 30, 10)
            };

            Canvas.SetLeft(label, x - 30);
            Canvas.SetTop(label, y - 10);

            return label;
        }

        private void AddCenter(double centerX, double centerY)
        {
            // 中心圆
            Ellipse centerCircle = new Ellipse
            {
                Width = 30,
                Height = 30,
                Fill = Brushes.Red,
                Stroke = Brushes.DarkRed,
                StrokeThickness = 3
            };

            Canvas.SetLeft(centerCircle, centerX - 15);
            Canvas.SetTop(centerCircle, centerY - 15);
            WheelCanvas.Children.Add(centerCircle);
        }

        private void OnWheelAngleChanged(float angle)
        {
            // 在UI线程上更新角度
            Dispatcher.Invoke(() =>
            {
                wheelTransform.Angle = angle;
            });
        }

        private void OnAnimationFrame(object sender, EventArgs e)
        {
            // 可以在这里添加额外的动画效果
        }

        private async void SpinButton_Click(object sender, RoutedEventArgs e)
        {
            if (isSpinning) return;
            if (!isInitialised)
            {
                MessageBoxX.Show("转盘未初始化，请点击重置后继续".Translate(), "警告".Translate());
            }
            isInitialised = false;
            isSpinning = true;
            SpinButton.IsEnabled = false;
            SpinButton.Content = "旋转中...".Translate();

            try
            {
                // 开始转盘旋转
                float finalAngle = await luckyWheel.StartWheel(60);

                // 显示结果
                int resultIndex = CalculateResult(finalAngle);
                ShowResult(prizes[resultIndex]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("转盘旋转出错: {0}".Translate(ex.Message), "错误".Translate(),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isSpinning = false;
                SpinButton.IsEnabled = true;
                SpinButton.Content = "开始旋转".Translate();
            }
        }

        private int CalculateResult(float finalAngle)
        {
            double anglePerSector = 360.0 / sectorCount;
            // 计算落在哪个扇形（考虑指针在顶部）
            int resultIndex = (int)((360 - finalAngle % 360) / anglePerSector) % sectorCount;
            return resultIndex;
        }

        private void ShowResult(string prize)
        {
            try
            {
                luckyWheel.WinCoin(Convert.ToUInt16(prize));
                MessageBoxX.Show("恭喜您获得: {0}!".Translate(prize), "抽奖结果".Translate());
            }
            catch (Exception ex)
            {
                MessageBox.Show("显示结果时出错: {0}".Translate(ex.Message), "错误".Translate(),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSpinning)
            {
                wheelTransform.Angle = 0;
                luckyWheel.PlaceCoin(coin, place, allPlace);
                isInitialised = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            animationTimer?.Stop();
        }

        private void WheelCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 当Canvas大小改变时重绘转盘
            if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                WheelCanvas.Children.Clear();
                DrawWheelSectors();
            }
        }

        private void ExitButton_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            base.Close();
        }

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
    }
}