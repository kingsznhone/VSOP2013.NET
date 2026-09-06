# XYZ → ELL 逆运算：数学原理与公式推导

作者：Claude Opus 4.6

## 概述

本文档记录从笛卡尔坐标 $(x, y, z, \dot{x}, \dot{y}, \dot{z})$ 逆运算到 VSOP2013 修正交点根数 $(a, L, k, h, q, p)$ 的完整数学推导。

这是 `ELLtoXYZ` 的逆运算。

位置以 AU 表示，速度以 AU/day 表示，恢复的 $a$ 以 AU 表示，角度以弧度表示，$\mu$ 的单位为 $\mathrm{AU}^3/\mathrm{day}^2$。位置和速度必须在同一个惯性参考系中表示；函数本身不旋转参考系。若要恢复标准 VSOP2013 根数，输入须以 J2000 动力学黄道与春分点为基准，不能直接混用 ICRS 赤道系坐标。

---

## 1. VSOP2013 交点根数定义

VSOP2013 使用一组采用 **$\sin(i/2)$ 参数化** 的椭圆变量（equinoctial variables）：

| 参数 | 含义 | 定义 |
|------|------|------|
| $a$ | 半长轴 | — |
| $L$ | 平经度 (Mean Longitude) | $L = M + \bar{\omega}$ |
| $k$ | 偏心率分量 | $k = e\cos\bar{\omega}$ |
| $h$ | 偏心率分量 | $h = e\sin\bar{\omega}$ |
| $q$ | 倾角分量 | $q = \sin(i/2)\cos\Omega$ |
| $p$ | 倾角分量 | $p = \sin(i/2)\sin\Omega$ |

其中 $\bar{\omega} = \Omega + \omega$ 是近日点经度，$\Omega$ 是升交点经度，$\omega$ 是近心点幅角，$M$ 是平近点角。

> **关键区别**：通常所称的 Modified Equinoctial Elements 使用 $\tan(i/2)$，VSOP2013 使用 $\sin(i/2)$。这直接影响 $q, p$ 的计算以及轨道面基向量的构造。

---

## 2. 引力参数

$$\mu = GM_\text{planet} + GM_\odot$$

当前 C# 实现为：

```csharp
double mu = GM[body] + GM[VSOPBody.SUN];
```

这里的 $GM$ 是标准引力参数，而非万有引力常数 $G$。逆运算必须使用与正运算相同的 `body` 和引力参数。

---

## 3. 半长轴 $a$

由 vis-viva 方程：

$$\frac{v^2}{2} - \frac{\mu}{r} = -\frac{\mu}{2a}$$

解出：

$$\boxed{a = \frac{\mu r}{2\mu - r v^2}}$$

其中 $r = \|\vec{r}\|$，$v^2 = \|\vec{v}\|^2$。

---

## 4. 倾角分量 $q, p$

### 4.1 比角动量向量

$$\vec{H} = \vec{r} \times \vec{v} = (H_x, H_y, H_z)$$

这里 $\vec H$ 是单位质量的角动量，后文简称角动量。其方向与有向轨道法线一致：

$$H_x = |\vec{H}|\sin i \sin\Omega, \quad H_y = -|\vec{H}|\sin i \cos\Omega, \quad H_z = |\vec{H}|\cos i$$

### 4.2 从角动量到 $q, p$

目标：$q = \sin(i/2)\cos\Omega$，$p = \sin(i/2)\sin\Omega$。

利用半角恒等式：

$$\sin(i/2) = \sqrt{\frac{1-\cos i}{2}} = \sqrt{\frac{|\vec{H}| - H_z}{2|\vec{H}|}}$$

以及 $\sin i = 2\sin(i/2)\cos(i/2)$，得：

$$\cos(i/2) = \sqrt{\frac{1+\cos i}{2}} = \sqrt{\frac{|\vec{H}| + H_z}{2|\vec{H}|}}$$

因此：

$$\sin(i/2)\cos\Omega = \frac{\sin i \cos\Omega}{2\cos(i/2)} = \frac{-H_y / |\vec{H}|}{2\cos(i/2)}$$

代入 $\cos(i/2)$ 的表达式：

$$\boxed{q = \frac{-H_y}{\sqrt{2|\vec{H}|(|\vec{H}|+H_z)}}}$$

$$\boxed{p = \frac{H_x}{\sqrt{2|\vec{H}|(|\vec{H}|+H_z)}}}$$

### 4.3 数值稳定性

在 $|\vec H|>0$ 的前提下，分母 $\sqrt{2|\vec{H}|(|\vec{H}|+H_z)}$ 在 $i=0$ 时等于 $2|\vec H|$，因此零倾角不会出现除零。精确算术下，分母仅在 $i=\pi$（$180^\circ$，精确逆行共面轨道，$H_z=-|\vec H|$）时为零；并非所有逆行轨道都有此奇点。

浮点算术下，接近 $i=\pi$ 时，$|\vec H|+H_z$ 会因两个近乎相反的数相加而损失有效数字，甚至提前舍入到零。因此，不能把“仅在 $180^\circ$ 时分母为零”理解为其余所有倾角都数值稳定。$|\vec H|=0$ 的径向运动同样不适用。

辅助量：

$$\chi = \sqrt{1 - q^2 - p^2} = \cos(i/2)$$

---

## 5. 轨道面基向量

在 `ELLtoXYZ` 的正向运算中，位置可以写成：

$$\vec{r} = r \left[(\cos F)\,\hat{e}_1 + (\sin F)\,\hat{e}_2\right]$$

其中 $F=\nu+\bar\omega$ 是真经度（True Longitude），$\hat{e}_1, \hat{e}_2$ 是轨道面内的正交基向量。$F$ 是该基底中的角度，通常不等于三维位置的黄经 $\operatorname{atan2}(y,x)$。

通过分析 `ELLtoXYZ` 中的位置公式（其结构包含 $q, p, \chi$ 的旋转），可以反推出基向量为：

$$\hat{e}_1 = \begin{pmatrix} 1 - 2p^2 \\ 2pq \\ -2p\chi \end{pmatrix}, \quad \hat{e}_2 = \begin{pmatrix} 2pq \\ 1 - 2q^2 \\ 2q\chi \end{pmatrix}$$

### 5.1 验证正交性

$$\hat{e}_1 \cdot \hat{e}_2 = 2pq(1-2p^2) + 2pq(1-2q^2) + (-2p\chi)(2q\chi)$$
$$= 2pq(2 - 2p^2 - 2q^2 - 2\chi^2) = 2pq(2 - 2(p^2+q^2+\chi^2)) = 0$$

因为 $p^2 + q^2 + \chi^2 = 1$。

### 5.2 验证模长

$$|\hat{e}_1|^2 = (1-2p^2)^2 + 4p^2q^2 + 4p^2\chi^2 = 1 - 4p^2 + 4p^4 + 4p^2(q^2+\chi^2)$$

$$= 1 - 4p^2 + 4p^2(p^2 + q^2 + \chi^2) = 1$$

同理可得 $|\hat e_2|^2=1$；因此投影回轨道面时可以直接使用这两个单位向量的点积。

---

## 6. 偏心率分量 $k, h$

### 6.1 偏心率向量

经典轨道力学中的偏心率向量：

$$\vec{e} = \frac{1}{\mu}\left[\left(v^2 - \frac{\mu}{r}\right)\vec{r} - (\vec{r}\cdot\vec{v})\vec{v}\right]$$

该向量指向近日点方向，$|\vec{e}| = e$（偏心率）。

### 6.2 投影到轨道面基向量

$k$ 和 $h$ 分别是偏心率向量在 $\hat{e}_1$ 和 $\hat{e}_2$ 方向上的分量：

$$\boxed{k = \vec{e} \cdot \hat{e}_1, \quad h = \vec{e} \cdot \hat{e}_2}$$

这等价于将偏心率向量从三维空间投影到轨道面坐标系中，其中 $\hat{e}_1$ 对应零经度方向。

验证：$e = \sqrt{k^2 + h^2}$；当 $e>0$ 时，$\bar{\omega} = \operatorname{atan2}(h, k)$（模 $2\pi$）。

辅助量：

$$\phi = \sqrt{1 - k^2 - h^2} = \sqrt{1 - e^2}$$

---

## 7. 平经度 $L$

这是最关键的一步，需要从位置推导出平经度。

### 7.1 真经度 $F$

将位置向量投影到轨道面基向量上：

$$\cos F = \frac{\vec{r} \cdot \hat{e}_1}{r}, \quad \sin F = \frac{\vec{r} \cdot \hat{e}_2}{r}$$

$$\boxed{F = \text{atan2}\!\left(\frac{\vec{r} \cdot \hat{e}_2}{r},\; \frac{\vec{r} \cdot \hat{e}_1}{r}\right)}$$

### 7.2 真近点角 $\nu$

真经度 = 近日点经度 + 真近点角（角度关系均按模 $2\pi$ 理解）：

$$F = \bar{\omega} + \nu \implies \nu = F - \bar{\omega}$$

其中 $\bar{\omega} = \operatorname{atan2}(h, k)$；圆轨道 $e=0$ 的处理见第 7.6 节。

### 7.3 偏近点角 $E'$（经典公式）

利用真近点角与偏近点角的精确关系：

$$\cos E' = \frac{e + \cos\nu}{1 + e\cos\nu}, \quad \sin E' = \frac{\sqrt{1-e^2}\sin\nu}{1 + e\cos\nu}$$

$$E' = \text{atan2}(\sin E', \cos E')$$

当 $0\leq e<1$ 时，分母 $1+e\cos\nu\geq1-e>0$。上述两式在精确算术下满足 $\cos^2 E'+\sin^2 E'=1$，浮点计算中仍有舍入误差。两者分母相同且为正，因此也可直接使用 $E'=\operatorname{atan2}(\phi\sin\nu,e+\cos\nu)$。

### 7.4 偏经度 $E$

$$E = E' + \bar{\omega}$$

### 7.5 平经度 $L$（交点形式的开普勒方程）

经典开普勒方程 $M = E' - e\sin E'$，在交点根数下变为：

$$\boxed{L = E - k\sin E + h\cos E}$$

**推导**：

将 $E = E' + \bar{\omega}$ 代入，利用 $k = e\cos\bar{\omega}$，$h = e\sin\bar{\omega}$：

$$k\sin E - h\cos E$$
$$= e\cos\bar{\omega}\sin(E'+\bar{\omega}) - e\sin\bar{\omega}\cos(E'+\bar{\omega})$$

展开：

$$= e[\cos\bar{\omega}\sin E'\cos\bar{\omega} + \cos\bar{\omega}\cos E'\sin\bar{\omega}]$$
$$\quad\; - e[\sin\bar{\omega}\cos E'\cos\bar{\omega} - \sin\bar{\omega}\sin E'\sin\bar{\omega}]$$

$$= e\sin E'(\cos^2\bar{\omega} + \sin^2\bar{\omega}) = e\sin E'$$

因此：

$$E - k\sin E + h\cos E = (E' + \bar{\omega}) - e\sin E' = (E' - e\sin E') + \bar{\omega} = M + \bar{\omega} = L$$

### 7.6 圆轨道与角度分支

当 $e=0$ 时，近日点经度 $\bar\omega$ 和真近点角 $\nu$ 各自没有唯一的几何定义，但 $k=h=0$，平经度 $L$ 仍有意义。任选辅助角 $\alpha$，令 $\nu=F-\alpha$，则 $E'\equiv\nu$、$E=E'+\alpha\equiv F$，最终 $L\equiv F\pmod{2\pi}$。因此，`atan2(h, k)` 在零偏心率时给出的辅助取值不影响最终的平经度（模 $2\pi$）。

`atan2` 返回主值，故上述中间角度可能相差整圈。当前实现最后执行：

```csharp
L = (L % Math.Tau + Math.Tau) % Math.Tau;
```

这将平经度规范到 $[0,2\pi)$。比较角度时应使用环绕差，例如 $\Delta L=\operatorname{atan2}(\sin(L_2-L_1),\cos(L_2-L_1))$，避免把跨越零点的两个相近角度判为相差一整圈。

---

## 8. 与 ELLtoXYZ 正向运算的对应关系

| 正向 (ELLtoXYZ) | 逆向 (XYZtoELL) |
|---|---|
| 已知 $L$，解开普勒方程求 $E$ | 已知位置，通过投影求 $F$，再精确转换到 $E$，最后用开普勒方程求 $L$ |
| 迭代求解 $L - E + \text{Im}(\bar{z}e^{iE}) = 0$ | 无需迭代（位置已知） |
| 通过 $q, p$ 构造旋转矩阵将轨道面坐标转到三维 | 通过 $q, p$ 构造 $\hat{e}_1, \hat{e}_2$ 将三维坐标投影回轨道面 |
| 输入 6 个根数，输出 6 个坐标 | 输入 6 个坐标，输出 6 个根数 |

---

## 9. 精度说明

- 逆运算不需要迭代，所有步骤都是解析的（仅涉及 `atan2`、`sqrt` 等初等函数）。
- 笛卡尔状态不能保留平经度的完整圈数；逆运算只能恢复 $L\bmod 2\pi$。实现将 $L$ 规范到 $[0,2\pi)$，与 `Calculator` 的输出约定一致。
- 对非退化椭圆轨道，`ELL → XYZ → ELL` 的非角度分量通常可达到接近双精度机器精度的误差；$L$ 必须按模 $2\pi$ 比较。接近 $e=1$、$i=\pi$ 或零角动量时，误差会放大或参数化失效。
- 该逆运算不适用于零距离、抛物线、双曲线、零角动量径向运动或倾角恰为 $180^\circ$ 的状态。
- 上述定义域是调用前提，不代表当前实现已经完整校验输入。退化或非法输入可能产生无穷大或 `NaN`，而不是明确的参数异常。
