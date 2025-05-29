using System;
using System.Data;

namespace Lab6;

public class CollisionDetector
{
    private const float BUBBLE_DIMS = Board.VISUAL_BUBBLE_DIAMETER;
    private const float BUBBLE_RADIUS = Board.BUBBLE_RADIUS;

    public static (Bubble bubble, float dist, (float x, float y) coords)? FindIntersection(
     Bubble curBubble,
     Board board,
     double angle,
     float startX,
     float startY)
    {
        (Bubble bubble, float dist, (float x, float y) coords)? nearestCollision = null;

        double dirX = Math.Sin(angle);
        double dirY = -Math.Cos(angle);

        foreach (var rowArray in board.GetRows())
        {
            if (rowArray == null) continue;
            foreach (var targetBubble in rowArray)
            {
                if (targetBubble == null ||
                    targetBubble == curBubble ||
                    targetBubble.GetState() != Bubble.BubbleState.OnBoard)
                {
                    continue;
                }
                var (targetX, targetY) = targetBubble.GetPosition();

                double Lx = startX - targetX;
                double Ly = startY - targetY;

                double tca = Lx * dirX + Ly * dirY;

                double distBetweenCentersSq = Lx * Lx + Ly * Ly;
                if (tca < 0 && distBetweenCentersSq > (BUBBLE_DIMS * BUBBLE_DIMS) - 1e-4f)
                {
                    continue;
                }

                double dSquare = distBetweenCentersSq - tca * tca;
                double R_sum_sq = BUBBLE_DIMS * BUBBLE_DIMS;

                if (dSquare < -1e-4f || dSquare > R_sum_sq + 1e-4f)
                {
                    continue;
                }
                if (dSquare < 0 && dSquare >= -1e-4f) dSquare = 0;

                double thc = Math.Sqrt(Math.Max(0, R_sum_sq - dSquare));
                double t0 = tca - thc;

                float currentDistToCollision;

                if (t0 < -1e-4f)
                {
                    if (distBetweenCentersSq <= R_sum_sq + 1e-4f)
                    {
                        currentDistToCollision = 0;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    currentDistToCollision = (float)t0;
                }

                float collisionPointX = startX + (float)(currentDistToCollision * dirX);
                float collisionPointY = startY + (float)(currentDistToCollision * dirY);
                var actualCollisionCoords = (x: collisionPointX, y: collisionPointY);

                if (!nearestCollision.HasValue || currentDistToCollision < nearestCollision.Value.dist)
                {
                    if (currentDistToCollision >= -1e-4f)
                    {
                        nearestCollision = (targetBubble, currentDistToCollision, actualCollisionCoords);
                    }
                }
            }
        }
        return nearestCollision;
    }
}
