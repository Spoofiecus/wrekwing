package com.wreckwing.game;

import android.graphics.Canvas;
import android.graphics.Paint;
import java.util.ArrayList;
import java.util.List;
import java.util.Random;

/** Stadium made of destructible sections + debris physics. */
public class Stadium {
    public static class Section {
        public float x, y, w, h;
        public float health, maxHealth;
        public int color;
        public boolean destroyed, vip;
        public float shakeT;
    }

    public static class Debris {
        public float x, y, vx, vy, size, rot, vrot, life = 3f;
        public int color;
    }

    public final List<Section> sections = new ArrayList<>();
    public final List<Debris> debris = new ArrayList<>();
    private final Random rnd = new Random();
    public float topY, baseY;
    public int destroyedCount, vipHits;

    /** Build stadium rows across the screen bottom. */
    public void build(float screenW, float screenH, float groundH) {
        sections.clear(); debris.clear();
        destroyedCount = 0; vipHits = 0;
        baseY = screenH - groundH;
        topY = baseY - screenH * 0.34f;
        int rows = 4, cols = 7;
        float gap = 4f;
        float sh = (baseY - topY - gap * (rows - 1)) / rows;
        float sw = (screenW - gap * (cols + 1)) / cols;
        int[] colors = {0xFF3A6EA5, 0xFF5B8FC7, 0xFF4A7AB0, 0xFF6FA0D8};
        for (int r = 0; r < rows; r++)
            for (int cI = 0; cI < cols; cI++) {
                Section s = new Section();
                s.x = gap + cI * (sw + gap);
                s.y = topY + r * (sh + gap);
                s.w = sw; s.h = sh;
                s.maxHealth = s.health = 20f + rnd.nextInt(30);
                s.color = colors[r % colors.length];
                s.vip = (r == 0 && (cI == 1 || cI == 3 || cI == 5));
                if (s.vip) s.color = 0xFFD8B24A;
                sections.add(s);
            }
    }

    /** Apply impact at point; returns points scored. */
    public int impact(float ix, float iy, float force) {
        int points = 0;
        float radius = 40f + force * 26f;
        for (Section s : sections) {
            if (s.destroyed) continue;
            float cx = s.x + s.w / 2f, cy = s.y + s.h / 2f;
            float dist = (float) Math.sqrt((cx - ix) * (cx - ix) + (cy - iy) * (cy - iy));
            if (dist < radius) {
                float dmg = force * 14f * (1f - dist / radius);
                s.health -= dmg;
                s.shakeT = 0.35f;
                if (s.health <= 0) {
                    s.destroyed = true;
                    destroyedCount++;
                    points += s.vip ? 500 : 100;
                    if (s.vip) vipHits++;
                    spawnDebris(s, force);
                }
            }
        }
        return points;
    }

    private void spawnDebris(Section s, float force) {
        int n = 5 + (int) (force * 2);
        for (int i = 0; i < n; i++) {
            Debris d = new Debris();
            d.x = s.x + rnd.nextFloat() * s.w;
            d.y = s.y + rnd.nextFloat() * s.h;
            d.vx = (rnd.nextFloat() - 0.5f) * 76f * force;
            d.vy = -rnd.nextFloat() * 64f * force;
            d.size = 6f + rnd.nextFloat() * 14f;
            d.rot = rnd.nextFloat() * 360f;
            d.vrot = (rnd.nextFloat() - 0.5f) * 500f;
            d.color = s.color;
            debris.add(d);
        }
        if (debris.size() > 90) debris.subList(0, debris.size() - 90).clear();
    }

    public void update(float dt) {
        for (Debris d : debris) {
            d.vy += 900f * dt;
            d.x += d.vx * dt; d.y += d.vy * dt;
            d.rot += d.vrot * dt;
            d.life -= dt;
            if (d.y > baseY - d.size / 2f) {
                d.y = baseY - d.size / 2f;
                d.vy *= -0.35f; d.vx *= 0.7f; d.vrot *= 0.6f;
                if (Math.abs(d.vy) < 40f) { d.vy = 0; d.vx *= 0.5f; }
            }
        }
        for (int i = debris.size() - 1; i >= 0; i--)
            if (debris.get(i).life <= 0) debris.remove(i);
        for (Section s : sections)
            if (s.shakeT > 0) s.shakeT -= dt;
    }

    public float destructionPercent() {
        return sections.isEmpty() ? 0f : destroyedCount * 100f / sections.size();
    }

    public void draw(Canvas c, Paint p) {
        for (Section s : sections) {
            float sx = s.x, sy = s.y;
            if (s.shakeT > 0) {
                sx += (float) Math.sin(s.shakeT * 60) * 4 * s.shakeT;
                sy += (0.35f - s.shakeT) * 6;
            }
            if (s.destroyed) {
                p.setColor(0xFF30343C);
                c.drawRect(sx, sy + s.h * 0.55f, sx + s.w, sy + s.h, p);
            } else {
                p.setColor(s.color);
                c.drawRect(sx, sy, sx + s.w, sy + s.h, p);
                float dmg = 1f - s.health / s.maxHealth;
                if (dmg > 0.25f) {
                    p.setColor(0x66000000);
                    c.drawRect(sx + s.w * dmg * 0.3f, sy + s.h * dmg * 0.4f,
                            sx + s.w - s.w * dmg * 0.3f, sy + s.h, p);
                }
                if (s.vip) {
                    p.setColor(0xFFFFF3C0);
                    c.drawCircle(sx + s.w / 2f, sy + s.h / 2f, Math.min(s.w, s.h) * 0.18f, p);
                }
            }
        }
        for (Debris d : debris) {
            c.save();
            c.translate(d.x, d.y); c.rotate(d.rot);
            p.setColor(d.color);
            c.drawRect(-d.size / 2f, -d.size / 2f, d.size / 2f, d.size / 2f, p);
            c.restore();
        }
    }
}
