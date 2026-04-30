namespace SimplePaint
{
    using System; 
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Windows.Forms;
    public partial class Form1 : Form
    {
        enum ToolType { Line, Rectangle, Circle } // 사용할 도형 타입

        private Bitmap canvasBitmap;          // 실제 그림이 저장되는 비트맵
        private Graphics canvasGraphics;      // 비트맵 위에 그리기 위한 객체

        private bool isDrawing = false;       // 현재 드래그 중인지 여부
        private Point startPoint;             // 드래그 시작점
        private Point endPoint;               // 드래그 끝점

        private ToolType currentTool = ToolType.Line; // 현재 선택된 도형
        private Color currentColor = Color.Black;     // 현재 색상
        private int currentLineWidth = 2;             // 현재 선 두께
        public Form1()
        {
            InitializeComponent();

            // 캔버스 초기화
            canvasBitmap = new Bitmap(picCanvas.Width, picCanvas.Height);
            canvasGraphics = Graphics.FromImage(canvasBitmap);
            canvasGraphics.Clear(Color.White); // 캔버스를 흰색으로 초기화

            picCanvas.Image = canvasBitmap; // 그린 그림을 화면(PictureBox)에 표시

            // 마우스 이벤트 연결
            picCanvas.MouseDown += PicCanvas_MouseDown;
            picCanvas.MouseMove += PicCanvas_MouseMove;
            picCanvas.MouseUp += PicCanvas_MouseUp;

            // 다시 그릴 때 Paint 이벤트 연결
            picCanvas.Paint += PicCanvas_Paint;

            // 도형 선택 버튼 이벤트
            btnLine.Click += btnLine_Click;
            btnRectangle.Click += btnRectangle_Click;
            btnCircle.Click += btnCircle_Click;

            // 색상 콤보박스 이벤트
            cmbColor.SelectedIndexChanged += cmbColor_SelectedIndexChanged;
            cmbColor.SelectedIndex = 0; // 기본값 Black

            // 선 두께 트랙바 설정
            trbLineWidth.Minimum = 1; //최소 선 두께
            trbLineWidth.Maximum = 10; //최대 선 두께
            trbLineWidth.Value = 2; //기본 선 두께
            trbLineWidth.ValueChanged += trbLineWidth_ValueChanged;
        }
        private void PicCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            isDrawing = true;           // 드래그 시작
            startPoint = e.Location;    // 시작점 저장
        }

        private void PicCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDrawing) return;     // 드래그 아닐 때 무시
            endPoint = e.Location;      // 현재 위치 갱신
            // 화면 다시 그리기 (미리보기)
            picCanvas.Invalidate();
        }

        private void PicCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isDrawing) return; // 그림 그리기와 상관 없는 마우스 업 이벤트 무시
            isDrawing = false;          // 드래그 종료
            endPoint = e.Location;

            // 실제 비트맵에 확정으로 그림 그리기
            using (Pen pen = new Pen(currentColor, currentLineWidth))
            {
                DrawShape(canvasGraphics, pen, startPoint, endPoint);
            }
            picCanvas.Invalidate(); // 다시 그리기 결과 반영, paint 이벤트 발생
        }
        private void PicCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (!isDrawing) return;
            // 미리보기용 점선 펜
            using (Pen previewPen = new Pen(currentColor, currentLineWidth)) // 미리보기용 펜 생성
            {
                previewPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash; // 점선 스타일
                DrawShape(e.Graphics, previewPen, startPoint, endPoint); // 미리보기 그리기
            }
        }
        private void DrawShape(Graphics g, Pen pen, Point p1, Point p2)
        {
            Rectangle rect = GetRectangle(p1, p2); // 두 점을 기준으로 사각형 영역 계산

            switch (currentTool) // 현재 선택된 도형에 따라 그리기
            {
                case ToolType.Line: // 직선 그리기
                    g.DrawLine(pen, p1, p2);
                    break;

                case ToolType.Rectangle: // 사각형 그리기
                    g.DrawRectangle(pen, rect);
                    break;

                case ToolType.Circle: // 원 그리기
                    g.DrawEllipse(pen, rect);
                    break;
            }
        }
        private Rectangle GetRectangle(Point p1, Point p2)
        {
            // 두 점을 기준으로 사각형 영역 계산
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p1.X - p2.X),
                Math.Abs(p1.Y - p2.Y)
            );
        }
        private void btnLine_Click(object sender, EventArgs e)
        {
            currentTool = ToolType.Line;
        }

        private void btnRectangle_Click(object sender, EventArgs e)
        {
            currentTool = ToolType.Rectangle;
        }

        private void btnCircle_Click(object sender, EventArgs e)
        {
            currentTool = ToolType.Circle;
        }
        private void cmbColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbColor.SelectedIndex)
            {
                case 0: // Black 검정
                    currentColor = Color.Black;
                    break;

                case 1: // Red 빨강
                    currentColor = Color.Red;
                    break;

                case 2: // Blue 파랑
                    currentColor = Color.Blue;
                    break;

                case 3: // Green 녹색
                    currentColor = Color.Green;
                    break;

                default:
                    currentColor = Color.Black;
                    break;
            }
        }
        private void trbLineWidth_ValueChanged(object sender, EventArgs e)
        {
            currentLineWidth = trbLineWidth.Value;
        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            // 저장할 그림 있는지 체크
            if (canvasBitmap == null)
            {
                MessageBox.Show("저장할 그림이 없습니다.");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG 파일 (*.png)|*.png|JPG 파일 (*.jpg)|*.jpg|BMP 파일 (*.bmp)|*.bmp";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                switch (sfd.FilterIndex)
                {
                    case 1:
                        canvasBitmap.Save(sfd.FileName, ImageFormat.Png);
                        break;
                    case 2:
                        canvasBitmap.Save(sfd.FileName, ImageFormat.Jpeg);
                        break;
                    case 3:
                        canvasBitmap.Save(sfd.FileName, ImageFormat.Bmp);
                        break;
                }
            }
        }
    }
}
