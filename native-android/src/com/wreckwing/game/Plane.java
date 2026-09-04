package com.wreckwing.game;

import android.graphics.Canvas;
import android.graphics.Paint;

/**
 * Player aircraft. Falls with gravity, steered by touch. Supports three
 * plane types with distinct stats/size/color so the player can "work up"
 * to heavier aircraft for bigger destruction.
 */
public class Plane {
    /** Three tiers of aircraft. */
    public enum Type {
        LIGHT("Cessna", 320f, 46f, 60f, 1.0f, 0xFFF0F0F0),
        JET("Airlink E190", 380f, 56f, 72f, 1.6f, 0xFFDC3C3C),
        HEAVY("A380", 260f, 78f, 96f, 2.4f, 0xFF4A7AB0);

        public final String name;
        public final float speed;      // px/s descent
        public final float w, h;       // size
        public final float dmgMult;    // impact damage multiplier
        public final int bodyColor;

        Type(String name, float speed, float w, float h, float dmg, int color) {
            this.name = name; this.speed = speed; this.w = w; this.h = h;
            this.dmgMult = dmg; this.bodyColor = color;
        }
    }

    public Type type = Type.LIGHT;
    public float x, y;
    public float targetX;
    public float speed;
    public float w, h;
    public float roll;             // visual roll (deg)
    public boolean crashed;

    public void select(Type t) {
        this.type = t;
        this.speed = t.speed;
        this.w = t.w;
        this.h = t.h;
    }

    public void reset(float startX, float startY) {
        x = startX; y = startY;
        targetX = startX;
        crashed = false; roll = 0f;
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
        y += speed * dt;
        float targetRoll = -(move / maxDx) * 28f;
        roll += (targetRoll - roll) * Math.min(1f, dt * 8f);
    }

    /** Impact force scaled by aircraft weight/damage multiplier. */
    public float impactForce() {
        return (speed / 100f) * type.dmgMult;
    }

    /** Simple stylized aircraft, nose-down, coloured by type. */
    public void draw(Canvas c, Paint p) {
        c.save();
        c.translate(x, y);
        c.rotate(180f + roll);
        p.setColor(type.bodyColor);
        c.drawRect(-w * 0.16f, -h / 2f, w * 0.16f, h / 2f, p);
        p.setColor(0xFFDC3C3C);
        c.drawRect(-w / 2f, -h * 0.15f, w / 2f, h * 0.10f, p);
        p.setColor(0xFFB0B0B0);
        c.drawRect(-w * 0.30f, h * 0.32f, w * 0.30f, h * 0.42f, p);
        p.setColor(0xFF64B4FF);
        c.drawRect(-w * 0.10f, -h * 0.38f, w * 0.10f, -h * 0.12f, p);
        c.restore();
    }
}