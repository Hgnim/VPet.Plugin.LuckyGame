using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace VPet.Plugin.LuckyGame.Controls
{
    public partial class LotteryNumberRoller : UserControl
    {
        #region 依赖属性
        public static readonly DependencyProperty MainNumbersProperty =
            DependencyProperty.Register("MainNumbers", typeof(List<int>), typeof(LotteryNumberRoller),
                new PropertyMetadata(new List<int> { 0, 0, 0, 0, 0, 0 }, OnMainNumbersChanged));

        public static readonly DependencyProperty SpecialNumbersProperty =
            DependencyProperty.Register("SpecialNumbers", typeof(List<int>), typeof(LotteryNumberRoller),
                new PropertyMetadata(new List<int> { 0, 0 }, OnSpecialNumbersChanged));

        public static readonly DependencyProperty MainMinValueProperty =
            DependencyProperty.Register("MainMinValue", typeof(int), typeof(LotteryNumberRoller),
                new PropertyMetadata(0));

        public static readonly DependencyProperty MainMaxValueProperty =
            DependencyProperty.Register("MainMaxValue", typeof(int), typeof(LotteryNumberRoller),
                new PropertyMetadata(30));

        public static readonly DependencyProperty SpecialMinValueProperty =
            DependencyProperty.Register("SpecialMinValue", typeof(int), typeof(LotteryNumberRoller),
                new PropertyMetadata(0));

        public static readonly DependencyProperty SpecialMaxValueProperty =
            DependencyProperty.Register("SpecialMaxValue", typeof(int), typeof(LotteryNumberRoller),
                new PropertyMetadata(9));

        public static readonly DependencyProperty IsRollingProperty =
            DependencyProperty.Register("IsRolling", typeof(bool), typeof(LotteryNumberRoller),
                new PropertyMetadata(false));
        #endregion

        #region 属性
        public List<int> MainNumbers
        {
            get => (List<int>)GetValue(MainNumbersProperty);
            set => SetValue(MainNumbersProperty, value);
        }

        public List<int> SpecialNumbers
        {
            get => (List<int>)GetValue(SpecialNumbersProperty);
            set => SetValue(SpecialNumbersProperty, value);
        }

        public int MainMinValue
        {
            get => (int)GetValue(MainMinValueProperty);
            set => SetValue(MainMinValueProperty, value);
        }

        public int MainMaxValue
        {
            get => (int)GetValue(MainMaxValueProperty);
            set => SetValue(MainMaxValueProperty, value);
        }

        public int SpecialMinValue
        {
            get => (int)GetValue(SpecialMinValueProperty);
            set => SetValue(SpecialMinValueProperty, value);
        }

        public int SpecialMaxValue
        {
            get => (int)GetValue(SpecialMaxValueProperty);
            set => SetValue(SpecialMaxValueProperty, value);
        }

        public bool IsRolling
        {
            get => (bool)GetValue(IsRollingProperty);
            set => SetValue(IsRollingProperty, value);
        }
        #endregion

        private readonly Random _random;
        private List<TextBlock> _mainNumberTexts;
        private List<TextBlock> _specialNumberTexts;
        private bool _isInitialized = false;

        // 动画控制变量
        private DispatcherTimer _animationTimer;
        private int _currentFrame = 0;
        private int TotalFrames = 120; // 总帧数（4秒，30fps）

        // 每个数字的动画状态
        private class NumberAnimationState
        {
            public int CurrentDisplayValue { get; set; }
            public int FinalValue { get; set; }
            public int MinValue { get; set; }
            public int MaxValue { get; set; }
            public bool IsStopped { get; set; }
            public int StopFrame { get; set; } = -1; // 停止的帧数，-1表示未停止
        }

        private List<NumberAnimationState> _mainNumberStates;
        private List<NumberAnimationState> _specialNumberStates;

        public LotteryNumberRoller()
        {
            InitializeComponent();
            _random = new Random();
            InitializeVisualElements();
            InitializeAnimationTimer();
        }

        private void InitializeAnimationTimer()
        {
            _animationTimer = new DispatcherTimer();
            _animationTimer.Interval = TimeSpan.FromMilliseconds(33); // 约30fps
            _animationTimer.Tick += OnAnimationTimerTick;
        }

        private void InitializeVisualElements()
        {
            _mainNumberTexts = new List<TextBlock>
            {
                MainNumber1Text, MainNumber2Text, MainNumber3Text,
                MainNumber4Text, MainNumber5Text, MainNumber6Text
            };

            _specialNumberTexts = new List<TextBlock>
            {
                SpecialNumber1Text, SpecialNumber2Text
            };

            // 初始化显示
            UpdateNumbersDisplay();
            _isInitialized = true;
        }

        private static void OnMainNumbersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LotteryNumberRoller)d;
            control.UpdateNumbersDisplay();
        }

        private static void OnSpecialNumbersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LotteryNumberRoller)d;
            control.UpdateNumbersDisplay();
        }

        private void UpdateNumbersDisplay()
        {
            if (!_isInitialized) return;

            // 安全更新主号码显示
            for (int i = 0; i < 6; i++)
            {
                if (i < _mainNumberTexts.Count && i < MainNumbers.Count)
                {
                    _mainNumberTexts[i].Text = MainNumbers[i].ToString();
                }
                else if (i < _mainNumberTexts.Count)
                {
                    _mainNumberTexts[i].Text = "0";
                }
            }

            // 安全更新副号码显示
            for (int i = 0; i < 2; i++)
            {
                if (i < _specialNumberTexts.Count && i < SpecialNumbers.Count)
                {
                    _specialNumberTexts[i].Text = SpecialNumbers[i].ToString();
                }
                else if (i < _specialNumberTexts.Count)
                {
                    _specialNumberTexts[i].Text = "0";
                }
            }
        }

        /// <summary>
        /// 开始滚动动画
        /// </summary>
        public void StartRollingAnimation(double durationSeconds = 4.0)
        {
            if (IsRolling || !_isInitialized)
                return;
            TotalFrames = (int)(durationSeconds * 30); // 根据持续时间调整总帧数
            IsRolling = true;
            _currentFrame = 0;

            // 验证号码数据
            ValidateNumbers();

            // 初始化动画状态
            InitializeAnimationStates();

            // 重置所有数字显示为随机值
            ResetNumberDisplays();

            // 恢复文本颜色
            ResetTextColors();

            // 启动动画定时器
            _animationTimer.Start();

            // 触发开始事件
            RollingStarted?.Invoke(this, EventArgs.Empty);
        }

        private void ValidateNumbers()
        {
            // 确保主号码有6个
            while (MainNumbers.Count < 6)
            {
                MainNumbers.Add(0);
            }

            // 确保副号码有2个
            while (SpecialNumbers.Count < 2)
            {
                SpecialNumbers.Add(0);
            }
        }

        private void InitializeAnimationStates()
        {
            _mainNumberStates = new List<NumberAnimationState>();
            _specialNumberStates = new List<NumberAnimationState>();

            // 初始化主号码动画状态
            for (int i = 0; i < 6; i++)
            {
                if (i < MainNumbers.Count)
                {
                    _mainNumberStates.Add(new NumberAnimationState
                    {
                        CurrentDisplayValue = _random.Next(MainMinValue, MainMaxValue + 1),
                        FinalValue = MainNumbers[i],
                        MinValue = MainMinValue,
                        MaxValue = MainMaxValue,
                        IsStopped = false,
                        StopFrame = -1
                    });
                }
            }

            // 初始化副号码动画状态
            for (int i = 0; i < 2; i++)
            {
                if (i < SpecialNumbers.Count)
                {
                    _specialNumberStates.Add(new NumberAnimationState
                    {
                        CurrentDisplayValue = _random.Next(SpecialMinValue, SpecialMaxValue + 1),
                        FinalValue = SpecialNumbers[i],
                        MinValue = SpecialMinValue,
                        MaxValue = SpecialMaxValue,
                        IsStopped = false,
                        StopFrame = -1
                    });
                }
            }
        }

        private void ResetNumberDisplays()
        {
            // 重置主号码显示为随机值
            for (int i = 0; i < 6 && i < _mainNumberTexts.Count && i < _mainNumberStates.Count; i++)
            {
                _mainNumberTexts[i].Text = _mainNumberStates[i].CurrentDisplayValue.ToString();
            }

            // 重置副号码显示为随机值
            for (int i = 0; i < 2 && i < _specialNumberTexts.Count && i < _specialNumberStates.Count; i++)
            {
                _specialNumberTexts[i].Text = _specialNumberStates[i].CurrentDisplayValue.ToString();
            }
        }

        private void ResetTextColors()
        {
            // 重置主号码颜色
            foreach (var textBlock in _mainNumberTexts)
            {
                textBlock.Foreground = (Brush)FindResource("SuccessBrush");
            }

            // 重置副号码颜色
            foreach (var textBlock in _specialNumberTexts)
            {
                textBlock.Foreground = (Brush)FindResource("DangerBrush");
            }
        }

        private void OnAnimationTimerTick(object sender, EventArgs e)
        {
            _currentFrame++;

            // 计算动画进度 (0.0 - 1.0)
            double progress = (double)_currentFrame / TotalFrames;

            // 更新数字显示
            UpdateNumberAnimations(progress);

            // 检查动画是否完成
            if (_currentFrame >= TotalFrames)
            {
                CompleteAnimation();
            }
        }

        private void UpdateNumberAnimations(double progress)
        {
            // 计算当前滚动速度
            int speed = CalculateCurrentSpeed(progress);

            // 更新主号码动画
            for (int i = 0; i < _mainNumberStates.Count; i++)
            {
                if (!_mainNumberStates[i].IsStopped)
                {
                    UpdateSingleNumberAnimation(_mainNumberStates[i], _mainNumberTexts[i], speed, progress, i);
                }
            }

            // 更新副号码动画（延迟开始）
            for (int i = 0; i < _specialNumberStates.Count; i++)
            {
                if (!_specialNumberStates[i].IsStopped && progress > 0.1) // 延迟10%开始
                {
                    UpdateSingleNumberAnimation(_specialNumberStates[i], _specialNumberTexts[i], speed, progress, i + 6);
                }
            }
        }

        private int CalculateCurrentSpeed(double progress)
        {
            if (progress < 0.7)
            {
                // 快速阶段：高速滚动
                return 5 + (int)(10 * (1 - progress)); // 开始快，逐渐稍慢
            }
            else
            {
                // 减速阶段：逐渐变慢
                double slowFactor = (1.0 - progress) / 0.3; // 从1.0到0.0
                return 1 + (int)(4 * slowFactor); // 从5降到1
            }
        }

        private void UpdateSingleNumberAnimation(NumberAnimationState state, TextBlock textBlock, int speed, double progress, int numberIndex)
        {
            // 检查是否应该停止在这个数字
            if (ShouldStopAtThisNumber(state, progress, numberIndex))
            {
                state.CurrentDisplayValue = state.FinalValue;
                state.IsStopped = true;
                state.StopFrame = _currentFrame;
                textBlock.Text = state.FinalValue.ToString();

                // 触发数字停止事件
                NumberStopped?.Invoke(this, new NumberStoppedEventArgs
                {
                    NumberIndex = numberIndex,
                    FinalValue = state.FinalValue
                });
                return;
            }

            // 继续滚动
            int newValue = state.CurrentDisplayValue + speed;

            newValue = (newValue + 1) % state.MaxValue;

            state.CurrentDisplayValue = newValue;
            textBlock.Text = newValue.ToString();

            // 接近目标值时添加视觉反馈
            if (progress > 0.8 && Math.Abs(newValue - state.FinalValue) <= 3)
            {
                textBlock.Foreground = Brushes.Gold;
            }
        }

        private bool ShouldStopAtThisNumber(NumberAnimationState state, double progress, int numberIndex)
        {
            if (state.IsStopped)
                return false;

            // 在减速阶段才有机会停止
            if (progress < 0.7)
                return false;

            // 计算停止概率
            double stopProbability = CalculateStopProbability(progress, numberIndex);

            // 检查当前显示值是否接近最终值
            bool isNearTarget = Math.Abs(state.CurrentDisplayValue - state.FinalValue) <= 2;

            // 如果接近目标值，增加停止概率
            if (isNearTarget)
            {
                stopProbability *= 2.0;
            }

            // 最后一个数字强制在动画结束时停止
            if (numberIndex == 7 && progress > 0.95)
            {
                return true;
            }

            return _random.NextDouble() < stopProbability;
        }

        private double CalculateStopProbability(double progress, int numberIndex)
        {
            // 基础停止概率
            double baseProbability = (progress - 0.7) / 0.3; // 从0到1

            // 根据数字位置调整概率（前面的数字先停）
            double positionFactor = 1.0 - (numberIndex * 0.1);

            return baseProbability * positionFactor * 0.3;
        }

        private void CompleteAnimation()
        {
            _animationTimer.Stop();
            IsRolling = false;

            // 确保所有数字都显示最终值
            for (int i = 0; i < _mainNumberStates.Count; i++)
            {
                _mainNumberStates[i].CurrentDisplayValue = _mainNumberStates[i].FinalValue;
                _mainNumberStates[i].IsStopped = true;
                if (i < _mainNumberTexts.Count)
                {
                    _mainNumberTexts[i].Text = _mainNumberStates[i].FinalValue.ToString();
                }
            }

            for (int i = 0; i < _specialNumberStates.Count; i++)
            {
                _specialNumberStates[i].CurrentDisplayValue = _specialNumberStates[i].FinalValue;
                _specialNumberStates[i].IsStopped = true;
                if (i < _specialNumberTexts.Count)
                {
                    _specialNumberTexts[i].Text = _specialNumberStates[i].FinalValue.ToString();
                }
            }

            // 恢复文本颜色
            ResetTextColors();

            // 触发完成事件
            RollingCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 停止滚动并立即显示最终结果
        /// </summary>
        public void StopRolling()
        {
            if (!IsRolling) return;

            _animationTimer.Stop();
            CompleteAnimation();
        }

        /// <summary>
        /// 设置最终数字
        /// </summary>
        public void SetFinalNumbers(byte[] mainNumbers, byte[] specialNumbers)
        {
            if (mainNumbers.Length == 6)
            {
                var newMainNumbers = new List<int>();
                foreach (var num in mainNumbers)
                    newMainNumbers.Add(Convert.ToInt32(num));
                MainNumbers = newMainNumbers;
            }

            if (specialNumbers.Length == 2)
            {
                var newSpecialNumbers = new List<int>();
                foreach (var num in specialNumbers)
                    newSpecialNumbers.Add(Convert.ToInt32(num));
                SpecialNumbers = newSpecialNumbers;
            }

            UpdateNumbersDisplay();
        }

        /// <summary>
        /// 随机生成号码
        /// </summary>
        public void GenerateRandomNumbers()
        {
            var mainNumbers = new List<int>();
            var specialNumbers = new List<int>();

            for (int i = 0; i < 6; i++)
            {
                mainNumbers.Add(_random.Next(MainMinValue, MainMaxValue + 1) % MainMaxValue);
            }

            for (int i = 0; i < 2; i++)
            {
                specialNumbers.Add(_random.Next(SpecialMinValue, SpecialMaxValue + 1) % SpecialMaxValue);
            }

            MainNumbers = mainNumbers;
            SpecialNumbers = specialNumbers;
            UpdateNumbersDisplay();
        }

        /// <summary>
        /// 清空所有号码
        /// </summary>
        public void ClearNumbers()
        {
            MainNumbers = new List<int> { 0, 0, 0, 0, 0, 0 };
            SpecialNumbers = new List<int> { 0, 0 };
            UpdateNumbersDisplay();
        }

        /// <summary>
        /// 滚动动画开始事件
        /// </summary>
        public event EventHandler RollingStarted;

        /// <summary>
        /// 滚动动画完成事件
        /// </summary>
        public event EventHandler RollingCompleted;

        /// <summary>
        /// 单个数字停止事件
        /// </summary>
        public event EventHandler<NumberStoppedEventArgs> NumberStopped;
    }

    /// <summary>
    /// 数字停止事件参数
    /// </summary>
    public class NumberStoppedEventArgs : EventArgs
    {
        public int NumberIndex { get; set; }     // 数字索引（0-5主号码，6-7副号码）
        public int FinalValue { get; set; }       // 最终显示的值
    }
}