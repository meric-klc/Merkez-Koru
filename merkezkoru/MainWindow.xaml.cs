using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.Generic;

namespace merkezkoru
{
    public partial class MainWindow : Window
    {
        Ellipse ball = new Ellipse();
        int formW = 450;  // Form genişliği
        int formH = 450;  // Form yüksekliği
        int ballW = 15;
        int ballH = 15;
        double ballSpeedX = 0;
        double ballSpeedY = 0;
        double acceleration = 0.8; // Hızlanma miktarı
        double maxSpeed = 10; // Maksimum hız
        double friction = 0.85; // Sürtünme miktarı
        double enmyspd = 0.2; // Sabit hız değeri

        bool isLeftPressed = false;
        bool isRightPressed = false;
        bool isUpPressed = false;
        bool isDownPressed = false;

        int lives = 3; // Oyuncunun başlangıç canı
        Random random = new Random();
        DispatcherTimer gameTimer = new DispatcherTimer();
        int activeRectangles = 0; // Aktif karelerin sayısını tutar


        private void UpdateLivesDisplay()
        {
            LivesTextBlock.Text = $"Canlar: {lives}";
        }

        public MainWindow()
        {
            InitializeComponent();
            Canvas1.Children.Add(ball);
            setBall();
            CreateCenterCircle(); // Orta noktada yuvarlak oluştur
            UpdateLivesDisplay(); // Canları başlarken güncelle
            gameTimer.Interval = TimeSpan.FromMilliseconds(15);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();
            this.KeyDown += Window_KeyDown;
            this.KeyUp += Window_KeyUp;
            CreateRandomObjects();
        }

        private void DecreaseLives()
        {
            lives--;
            UpdateLivesDisplay(); // Canlar azaldığında güncelle
            if (lives == 0)
            {
                MessageBox.Show("Oyun Bitti!");
                Application.Current.Shutdown(); // Oyunu kapat
            }
        }



        private void setBall()
        {
            ball.Width = ballW;
            ball.Height = ballH;
            ball.Stroke = Brushes.Black;
            ball.Fill = Brushes.Blue;
            Canvas.SetLeft(ball, formW / 2 - ball.Width / 2); // Topun merkezi ortada olacak şekilde konumlandır
            Canvas.SetTop(ball, formH / 2 - ball.Height / 2);
        }

        private void CreateCenterCircle()
        {
            Ellipse centerCircle = new Ellipse
            {
                Width = 50,
                Height = 50,
                Fill = Brushes.Black
            };

            double centerX = (formW - centerCircle.Width) / 2;
            double centerY = (formH - centerCircle.Height) / 2;

            Canvas.SetLeft(centerCircle, centerX);
            Canvas.SetTop(centerCircle, centerY);

            Canvas1.Children.Add(centerCircle);
        }

        private void CreateRandomObjects()
        {
            List<Rectangle> rectangles = new List<Rectangle>(); // Mevcut karelerin listesini tutar

            for (int i = 0; i < 5; i++) // 5 adet rastgele cisim oluşturuyoruz
            {
                Rectangle rect = new Rectangle
                {
                    Width = 20,
                    Height = 20,
                    Fill = Brushes.Red
                };

                double x, y;
                bool isValidPosition;

                do
                {
                    x = 0; y = 0;
                    isValidPosition = true;
                    int side = random.Next(4); // Hangi kenardan geleceğini belirliyoruz (0=üst, 1=alt, 2=sol, 3=sağ)
                    switch (side)
                    {
                        case 0: // Üst kenar
                            x = random.Next(0, formW);
                            y = 0;
                            break;
                        case 1: // Alt kenar
                            x = random.Next(0, formW);
                            y = formH - rect.Height;
                            break;
                        case 2: // Sol kenar
                            x = 0;
                            y = random.Next(0, formH);
                            break;
                        case 3: // Sağ kenar
                            x = formW - rect.Width;
                            y = random.Next(0, formH);
                            break;
                    }

                    // Mevcut karelerle çakışmayı kontrol et
                    foreach (Rectangle existingRect in rectangles)
                    {
                        double existingX = Canvas.GetLeft(existingRect);
                        double existingY = Canvas.GetTop(existingRect);
                        if (Math.Abs(existingX - x) < rect.Width && Math.Abs(existingY - y) < rect.Height)
                        {
                            isValidPosition = false;
                            break;
                        }
                    }
                } while (!isValidPosition); // Geçerli bir pozisyon bulunana kadar devam et

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                Canvas1.Children.Add(rect);
                rectangles.Add(rect); // Yeni kareyi listeye ekle
                activeRectangles++; // Aktif kare sayısını artır

                // Cismi merkeze doğru hareket ettirelim
                DispatcherTimer moveTimer = new DispatcherTimer();
                moveTimer.Interval = TimeSpan.FromMilliseconds(15);
                moveTimer.Tick += (sender, e) => MoveObject(rect);
                moveTimer.Start();
            }
        }

        private void MoveObject(Rectangle rect)
        {
            double x = Canvas.GetLeft(rect);
            double y = Canvas.GetTop(rect);
            double centerX = formW / 2 - rect.Width / 2;
            double centerY = formH / 2 - rect.Height / 2;

            double dx = centerX - x;
            double dy = centerY - y;

            // Normalize hareket vektörünü oluştur
            double distance = Math.Sqrt(dx * dx + dy * dy);
            dx = (dx / distance) * enmyspd;
            dy = (dy / distance) * enmyspd;

            Canvas.SetLeft(rect, x + dx);
            Canvas.SetTop(rect, y + dy);

            // Siyah daireye girince yok et ve can azalt
            if (distance < 25) // Siyah dairenin yarıçapı içinde kontrol
            {
                if (Canvas1.Children.Contains(rect)) // Rectangle mevcutsa kaldır
                {
                    Canvas1.Children.Remove(rect);
                    activeRectangles--;
                    DecreaseLives(); // Canı yalnızca burada azalt
                }
            }
        }


        private void GameLoop(object sender, EventArgs e)
        {
            MoveBall();
            BringBallToFront(); // Her oyun döngüsünde topu en öne getir

            // Tüm kareler yok olduğunda yeni kareler oluştur ve hızı artır
            if (activeRectangles == 0)
            {
                enmyspd += 0.2; // Hızı artır
                CreateRandomObjects();
            }
        }

        private void MoveBall()
        {
            if (isLeftPressed)
                ballSpeedX = Math.Max(ballSpeedX - acceleration, -maxSpeed);
            if (isRightPressed)
                ballSpeedX = Math.Min(ballSpeedX + acceleration, maxSpeed);
            if (isUpPressed)
                ballSpeedY = Math.Max(ballSpeedY - acceleration, -maxSpeed);
            if (isDownPressed)
                ballSpeedY = Math.Min(ballSpeedY + acceleration, maxSpeed);

            // Hızı yavaşlat
            ballSpeedX *= friction;
            ballSpeedY *= friction;

            double x = Canvas.GetLeft(ball) + ballSpeedX;
            double y = Canvas.GetTop(ball) + ballSpeedY;

            if (x < 0) x = 0;
            if (x > formW - ballW) x = formW - ballW;
            if (y < 0) y = 0;
            if (y > formH - ballH) y = formH - ballH;

            Canvas.SetLeft(ball, x);
            Canvas.SetTop(ball, y);
        }

        private void BringBallToFront()
        {
            Canvas1.Children.Remove(ball);
            Canvas1.Children.Add(ball);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    isLeftPressed = true;
                    break;
                case Key.Right:
                    isRightPressed = true;
                    break;
                case Key.Up:
                    isUpPressed = true;
                    break;
                case Key.Down:
                    isDownPressed = true;
                    break;
                case Key.Space:
                    CheckCollision();
                    break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    isLeftPressed = false;
                    break;
                case Key.Right:
                    isRightPressed = false;
                    break;
                case Key.Up:
                    isUpPressed = false;
                    break;
                case Key.Down:
                    isDownPressed = false;
                    break;
            }
        }

        private void CheckCollision()
        {
            double ballX = Canvas.GetLeft(ball) + ball.Width / 2;
            double ballY = Canvas.GetTop(ball) + ball.Height / 2;

            for (int i = Canvas1.Children.Count - 1; i >= 0; i--)
            {
                if (Canvas1.Children[i] is Rectangle rect)
                {
                    double rectX = Canvas.GetLeft(rect) + rect.Width / 2;
                    double rectY = Canvas.GetTop(rect) + rect.Height / 2;

                    double distance = Math.Sqrt(Math.Pow(ballX - rectX, 2) + Math.Pow(ballY - rectY, 2));
                    if (distance < (ball.Width / 2 + rect.Width / 2))
                    {
                        Canvas1.Children.Remove(rect);
                        activeRectangles--; // Aktif kare sayısını azalt
                    }
                }
            }
        }
    }
}
