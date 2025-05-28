namespace Lab6;

public class CollisionDetector
{
    private const float BUBBLE_DIMS = 50f;

    public static (Bubble bubble, float dist, (float x, float y) coords)? FindIntersection(Bubble curBubble, Board board, double angle, float startX, float startY)
    {
        var rows = board.GetRows();
        (Bubble bubble, float dist, (float x, float y) coords)? collision = null;
        var dx = Math.Sin(angle);
        var dy = -Math.Cos(angle);

        for (int i = 0; i < rows.Length; i++)
        {
            var row = rows[i];
            for (int j = 0; j < row.Length; j++)
            {
                var bubble = row[j];
                if (bubble != null)
                {
                    var (bubbleX, bubbleY) = bubble.GetPosition();
                    var distToBubbleX = startX - bubbleX;
                    var distToBubbleY = startY - bubbleY;
                    var t = dx * distToBubbleX + dy * distToBubbleY;
                    var ex = -t * dx + startX;
                    var ey = -t * dy + startY;
                    var distEC = Math.Sqrt((ex - bubbleX) * (ex - bubbleX) + (ey - bubbleY) * (ey - bubbleY));

                    if (distEC < BUBBLE_DIMS * 0.75)
                    {
                        var dt = Math.Sqrt(BUBBLE_DIMS * BUBBLE_DIMS - distEC * distEC);
                        var offset1 = (x: (t - dt) * dx, y: -(t - dt) * dy);
                        var offset2 = (x: (t + dt) * dx, y: -(t + dt) * dy);
                        var distToCollision1 = Math.Sqrt(offset1.x * offset1.x + offset1.y * offset1.y);
                        var distToCollision2 = Math.Sqrt(offset2.x * offset2.x + offset2.y * offset2.y);

                        if (distToCollision1 < distToCollision2)
                        {
                            var dest = (x: (float)(offset1.x + startX), y: (float)(offset1.y + startY));
                            if (!collision.HasValue || collision.Value.dist > distToCollision1)
                                collision = (bubble, (float)distToCollision1, dest);
                        }
                        else
                        {
                            var dest = (x: (float)(-offset2.x + startX), y: (float)(offset2.y + startY));
                            if (!collision.HasValue || collision.Value.dist > distToCollision2)
                                collision = (bubble, (float)distToCollision2, dest);
                        }
                    }
                }
            }
        }
        return collision;
    }
}