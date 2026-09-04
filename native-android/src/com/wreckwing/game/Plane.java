package com.wreckwing.game;

import android.graphics.Canvas;
import android.graphics.Paint;

/** Player aircraft: falls with gravity, steered by touch or tilt. */
public class Plane {
    public float x, y;
    public float vy;               // vertical speed
    public float targetX;          // steering target
    public float speed = 320f;     // px/s descent
    public float w = 46f, h = 60f;
    public float roll;             // visual roll angle (deg)
    public boolean alive = true;
    public boolean crashed = false;

    public void reset(float startX, float startY) {
        x = startX; y = startY;
        vy = speed;
        targetX = startX;
        alive = true; crashed = false; roll = 0f;
    }

    /** Steer toward target x with limited horizontal speed. */
    public void update(float dt, float screenW) {
        if (crashed) return;
        float dx = targetX - x;
        float maxDx = 620f * dt;
        float move = Math.max(-maxDx, Math.min(maxDx, dx));
        x += move;
        if (x < w) x = w;
        if (x > screenW - w) x = screenW - w;
        y += vy * dt;
        // visual roll follows horizontal velocity
        float targetRoll = -(move / maxDx) * 28f;
        roll += (targetRoll - roll) * Math.min(1f, dt * 8f);
    }

    /** Draw simple stylized jet, nose-down. */
    public void draw(Canvas c, Paint p) {
        c.save();
        c.translate(x, y);
        c.rotate(180f + roll); // nose points down
        // fuselage
        p.setColor(0xFFF0F0F0);
        c.drawRect(-w * 0.16f, -h / 2f, w * 0.16f, h / 2f, p);
        // wings (red)
        p.setColor(0xFFDC3C3C);
        c.drawRect(-w / 2f, -h * 0.15f, w / 2f, h * 0.10f, p);
        // tail
        p.setColor(0xFFB0B0B0);
        c.drawRect(-w * 0.30f, h * 0.32f, w * 0.30f, h * 0.42f, p);
        // cockpit
        p.setColor(0xFF64B4FF);
        c.drawRect(-w * 0.10f, -h * 0.38f, w * 0.10f, -h * 0.12f, p);
        c.restore();
    }

    /** Estimated impact force from current vertical speed. */
    public float impactForce() {
        return vy / 100f;
    }
}
