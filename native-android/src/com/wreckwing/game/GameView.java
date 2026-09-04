package com.wreckwing.game;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.LinearGradient;
import android.graphics.Paint;
import android.graphics.Shader;
import android.os.Build;
import android.os.VibrationEffect;
import android.os.Vibrator;
import android.view.MotionEvent;
import android.view.SurfaceHolder;
import android.view.SurfaceView;
import java.util.Random;

public class GameView extends SurfaceView implements SurfaceHolder.Callback, Runnable {
    private SurfaceHolder holder;
    private Thread gameThread;
    private volatile boolean running = false;
    private final Plane plane = new Plane();
    private final Stadium stadium = new Stadium();
    private final Paint paint = new Paint(Paint.ANTI_ALIAS_FLAG);
    private final Paint skyPaint = new Paint();
    private final Random rnd = new Random();

    private float screenW, screenH;
    private final float groundH = 90f;

    private int score, combo, comboTimer;
    private float comboMult = 1f;

    private float shakeT, shakeMag;
    private int gameState = 0; // 0=READY 1=FLYING 2=IMPACT 3=GAMEOVER
    private float stateTimer;
    private long lastTime;

    public GameView(Context context) {
        super(context);
        holder = getHolder();
        holder.addCallback(this);
        setFocusable(true);
        skyPaint.setShader(new LinearGradient(0, 0, 0, 1920, 0xFF87CEEB, 0xFFE0F0FF, Shader.TileMode.CLAMP));
    }

    @Override
    public void surfaceCreated(SurfaceHolder h) {
        if (gameThread == null) {
            running = true;
            lastTime = System.nanoTime();
            gameThread = new Thread(this);
            gameThread.start();
        }
    }

    @Override
    public void surfaceChanged(SurfaceHolder h, int format, int w, int hgt) {
        screenW = w;
        screenH = hgt;
        skyPaint.setShader(new LinearGradient(0, 0, 0, screenH, 0xFF87CEEB, 0xFFE0F0FF, Shader.TileMode.CLAMP));
        stadium.build(w, hgt, groundH);
        gameState = 0;
    }

    @Override
    public void surfaceDestroyed(SurfaceHolder h) {
        running = false;
        if (gameThread != null) {
            try { gameThread.join(1000); } catch (InterruptedException ignored) {}
            gameThread = null;
        }
    }

    @Override
    public void run() {
        while (running) {
            long now = System.nanoTime();
            float dt = (now - lastTime) / 1_000_000_000f;
            lastTime = now;
            if (dt > 0.05f) dt = 0.05f;
            if (screenW > 0) { update(dt); render(); }
            try { Thread.sleep(8); } catch (InterruptedException e) { return; }
        }
    }

    @Override
    public boolean onTouchEvent(MotionEvent event) {
        switch (event.getActionMasked()) {
            case MotionEvent.ACTION_DOWN:
            case MotionEvent.ACTION_MOVE:
                if (gameState == 0) {
                    gameState = 1;
                    plane.reset(screenW / 2f, 60);
                    combo = 0;
                    comboMult = 1f;
                }
                if (gameState == 1) plane.targetX = event.getX();
                break;
            case MotionEvent.ACTION_UP:
                if (gameState == 3) resetGame();
                break;
        }
        return true;
    }

    private void resetGame() {
        stadium.build(screenW, screenH, groundH);
        score = 0;
        combo = 0;
        comboMult = 1f;
        gameState = 0;
    }

    private void update(float dt) {
        if (shakeT > 0) shakeT -= dt;
        if (gameState == 0) {
            plane.reset(screenW / 2f, 60);
            plane.roll = (float) Math.sin(System.nanoTime() / 4e8) * 6f;
        } else if (gameState == 1) {
            plane.update(dt, screenW);
            if (plane.y + plane.h / 2f >= stadium.topY) {
                gameState = 2;
                stateTimer = 1.6f;
                plane.crashed = true;
                float force = plane.impactForce();
                score += (int) (stadium.impact(plane.x, stadium.topY, force) * comboMult);
                combo++;
                comboMult = Math.min(3f, 1f + combo * 0.25f);
                comboTimer = 2;
                shakeT = 0.5f;
                shakeMag = 12f * force;
                vibrate();
            }
        } else {
            stateTimer -= dt;
            stadium.update(dt);
            if (gameState == 2 && stateTimer <= 0) {
                if (stadium.destructionPercent() >= 100) score += 5000;
                gameState = 3;
                stateTimer = 0;
            }
        }
        if (combo > 0) {
            comboTimer -= (int) Math.ceil(dt);
            if (comboTimer <= 0) { combo = 0; comboMult = 1f; }
        }
    }

    private void vibrate() {
        try {
            Vibrator v = (Vibrator) getContext().getSystemService(Context.VIBRATOR_SERVICE);
            if (v == null) return;
            if (Build.VERSION.SDK_INT >= 26) {
                v.vibrate(VibrationEffect.createOneShot(120, 180));
            } else {
                v.vibrate(120);
            }
        } catch (Exception ignored) {}
    }

    public void pause() {
        running = false;
        if (gameThread != null) {
            try { gameThread.join(1000); } catch (InterruptedException ignored) {}
            gameThread = null;
        }
    }

    public void resume() {
        if (gameThread == null) {
            running = true;
            lastTime = System.nanoTime();
            gameThread = new Thread(this);
            gameThread.start();
        }
    }

    private void render() {
        Canvas c = null;
        try { c = holder.lockCanvas(); } catch (Exception ignored) {}
        if (c == null) return;
        try {
            boolean shake = shakeT > 0 && (gameState == 2 || gameState == 3);
            if (shake) c.save();
            if (shake) c.translate((rnd.nextFloat() * 2 - 1) * shakeMag, (rnd.nextFloat() * 2 - 1) * shakeMag);
            c.drawRect(0, 0, screenW, screenH, skyPaint);
            drawClouds(c);
            stadium.draw(c, paint);
            drawGround(c);
            drawPlane(c);
            drawHud(c);
            if (shake) c.restore();
        } finally {
            try { holder.unlockCanvasAndPost(c); } catch (Exception ignored) {}
        }
    }

    private void drawClouds(Canvas c) {
        paint.setColor(Color.WHITE);
        float t = (System.nanoTime() / 100000000f) * 10f;
        for (int i = 0; i < 3; i++) {
            float x = ((i * 260f + t) % (screenW + 160)) - 80;
            float y = 140 + i * 110;
            c.drawCircle(x, y, 38, paint);
            c.drawCircle(x + 34, y + 8, 28, paint);
            c.drawCircle(x - 30, y + 10, 24, paint);
        }
    }

    private void drawGround(Canvas c) {
        paint.setColor(0xFF1E5B1E);
        c.drawRect(0, stadium.baseY, screenW, screenH, paint);
        paint.setColor(Color.WHITE);
        paint.setStyle(Paint.Style.STROKE);
        paint.setStrokeWidth(3);
        c.drawLine(0, stadium.baseY + 20, screenW, stadium.baseY + 20, paint);
        c.drawLine(0, screenH - 20, screenW, screenH - 20, paint);
        c.drawCircle(screenW / 2f, (stadium.baseY + screenH) / 2f, 26, paint);
        paint.setStyle(Paint.Style.FILL);
    }

    private void drawPlane(Canvas c) {
        if (plane.crashed && stateTimer < 1.2f) {
            paint.setColor(0xFF444444);
            c.drawCircle(plane.x, stadium.topY, 22, paint);
            for (int i = 0; i < 5; i++) {
                paint.setColor(0x66999999);
                c.drawCircle(plane.x + (i - 2) * 8,
                        stadium.topY - 20 - i * 30 - (1.2f - stateTimer) * 40,
                        10 + i * 3, paint);
            }
        } else {
            plane.draw(c, paint);
        }
    }

    private void drawHud(Canvas c) {
        paint.setShadowLayer(4, 0, 0, Color.BLACK);
        paint.setColor(Color.WHITE);
        paint.setTextSize(48);
        paint.setFakeBoldText(true);
        paint.setTextAlign(Paint.Align.LEFT);
        c.drawText(String.valueOf(score), 24, 62, paint);
        paint.setTextAlign(Paint.Align.CENTER);
        c.drawText("DESTROYED " + stadium.destructionPercent() + "%", screenW / 2f, 110, paint);
        if (combo > 1) {
            paint.setColor(0xFFFFD700);
            paint.setTextAlign(Paint.Align.RIGHT);
            c.drawText("COMBO x" + comboMult, screenW - 24, 62, paint);
        }
        if (gameState == 0) {
            paint.setTextAlign(Paint.Align.CENTER);
            paint.setColor(Color.WHITE);
            paint.setTextSize(40);
            paint.setAlpha((int) (155 + 100 * Math.abs(Math.sin(System.nanoTime() / 4e8))));
            c.drawText("TAP TO START", screenW / 2f, screenH / 2f + 180, paint);
            paint.setAlpha(255);
        } else if (gameState == 3) {
            paint.setColor(0x99000000);
            c.drawRect(0, 0, screenW, screenH, paint);
            paint.setColor(Color.WHITE);
            paint.setTextSize(72);
            paint.setTextAlign(Paint.Align.CENTER);
            c.drawText("GAME OVER", screenW / 2f, screenH / 2f - 60, paint);
            paint.setTextSize(44);
            c.drawText("SCORE " + score, screenW / 2f, screenH / 2f + 10, paint);
            c.drawText("DESTRUCTION " + stadium.destructionPercent() + "%", screenW / 2f, screenH / 2f + 70, paint);
            paint.setAlpha((int) (155 + 100 * Math.abs(Math.sin(System.nanoTime() / 4e8))));
            paint.setTextSize(36);
            c.drawText("TAP TO FLY AGAIN", screenW / 2f, screenH / 2f + 150, paint);
            paint.setAlpha(255);
        }
        paint.setShadowLayer(0, 0, 0, Color.TRANSPARENT);
    }
}
