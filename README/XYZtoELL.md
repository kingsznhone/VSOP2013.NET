# XYZ → ELL 逆运算：数学原理与公式推导

作者 : Claude Opus 4.6

## 概述

本文档记录从笛卡尔坐标 $(x, y, z, \dot{x}, \dot{y}, \dot{z})$ 逆运算到 VSOP2013 修正交点根数 $(a, L, k, h, q, p)$ 的完整数学推导。

这是 `ELLtoXYZ` 的逆运算。

---

## 1. VSOP2013 交点根数定义

VSOP2013 使用 **$\sin(i/2)$ 约定** 的修正交点根数（Modified Equinoctial Elements）：

| 参数 | 含义 | 定义 |
|------|------|------|
| $a$ | 半长轴 | — |
| $L$ | 平经度 (Mean Longitude) | $L = M + \bar{\omega}$ |
| $k$ | 偏心率分量 | $k = e\cos\bar{\omega}$ |
| $h$ | 偏心率分量 | $h = e\sin\bar{\omega}$ |
| $q$ | 倾角分量 | $q = \sin(i/2)\cos\Omega$ |
| $p$ | 倾角分量 | $p = \sin(i/2)\sin\Omega$ |

其中 $\bar{\omega} = \Omega + \omega$ 是经度近日点，$M$ 是平近点角。

> **关键区别**：经典交点根数使用 $\tan(i/2)$，VSOP2013 使用 $\sin(i/2)$。这直接影响 $q, p$ 的计算以及轨道面基向量的构造。

---

## 2. 引力参数

$$\mu = GM_\text{planet} + GM_\odot = \texttt{gmp[(int)body]} + \texttt{gmsol}$$

注意 `gmsol` 是太阳的引力常数，而非 `gmp[0]`（水星）。

---

## 3. 半长轴 $a$

由 vis-viva 方程：

$$\frac{v^2}{2} - \frac{\mu}{r} = -\frac{\mu}{2a}$$

解出：

$$\boxed{a = \frac{\mu r}{2\mu - r v^2}}$$

其中 $r = \|\vec{r}\|$，$v^2 = \|\vec{v}\|^2$。

---

## 4. 倾角分量 $q, p$

### 4.1 角动量向量

$$\vec{H} = \vec{r} \times \vec{v} = (H_x, H_y, H_z)$$

角动量方向与轨道法线一致：

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

分母 $\sqrt{2|\vec{H}|(|\vec{H}|+H_z)}$ 在 $i=0$ 时等于 $\sqrt{2|\vec{H}| \cdot 2|\vec{H}|} = 2|\vec{H}|$，不会出现除零。仅当 $i = 180°$（逆行轨道，$H_z = -|\vec{H}|$）时分母为零，但太阳系行星不存在此情况。

辅助量：

$$\chi = \sqrt{1 - q^2 - p^2} = \cos(i/2)$$

---

## 5. 轨道面基向量

在 `ELLtoXYZ` 的正向运算中，位置可以写成：

$$\vec{r} = r \left[(\cos F)\,\hat{e}_1 + (\sin F)\,\hat{e}_2\right]$$

其中 $F$ 是真经度（True Longitude），$\hat{e}_1, \hat{e}_2$ 是轨道面内的正交基向量。

通过分析 `ELLtoXYZ` 中的位置公式（其结构包含 $q, p, \chi$ 的旋转），可以反推出基向量为：

$$\hat{e}_1 = \begin{pmatrix} 1 - 2p^2 \\ 2pq \\ -2p\chi \end{pmatrix}, \quad \hat{e}_2 = \begin{pmatrix} 2pq \\ 1 - 2q^2 \\ 2q\chi \end{pmatrix}$$

### 5.1 验证正交性

$$\hat{e}_1 \cdot \hat{e}_2 = 2pq(1-2p^2) + 2pq(1-2q^2) + (-2p\chi)(2q\chi)$$
$$= 2pq(2 - 2p^2 - 2q^2 - 2\chi^2) = 2pq(2 - 2(p^2+q^2+\chi^2)) = 0$$

因为 $p^2 + q^2 + \chi^2 = 1$。✓

### 5.2 验证模长

$$|\hat{e}_1|^2 = (1-2p^2)^2 + 4p^2q^2 + 4p^2\chi^2 = 1 - 4p^2 + 4p^4 + 4p^2(q^2+\chi^2)$$

$$= 1 - 4p^2 + 4p^2(p^2 + q^2 + \chi^2) = 1$$  

✓

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

验证：$e = \sqrt{k^2 + h^2}$，$\bar{\omega} = \text{atan2}(h, k)$。

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

真经度 = 近日点经度 + 真近点角：

$$F = \bar{\omega} + \nu \implies \nu = F - \bar{\omega}$$

其中 $\bar{\omega} = \text{atan2}(h, k)$。

### 7.3 偏近点角 $E'$（经典公式）

利用真近点角与偏近点角的精确关系：

$$\cos E' = \frac{e + \cos\nu}{1 + e\cos\nu}, \quad \sin E' = \frac{\sqrt{1-e^2}\sin\nu}{1 + e\cos\nu}$$

$$E' = \text{atan2}(\sin E', \cos E')$$

> 这个公式是精确的 Möbius 变换，保证了 $|\exp(iE')|$ 的单位模性质。

### 7.4 偏经度 $E$

$$E = E' + \bar{\omega}$$

### 7.5 平经度 $L$（交点形式的开普勒方程）

经典开普勒方程 $M = E' - e\sin E'$，在交点根数下变为：

$$\boxed{L = E - k\sin E + h\cos E}$$

**推导**：

将 $E = E' + \bar{\omega}$ 代入，利用 $k = e\cos\bar{\omega}$，$h = e\sin\bar{\omega}$：

$$k\sin E + h\cos E$$
$$= e\cos\bar{\omega}\sin(E'+\bar{\omega}) - e\sin\bar{\omega}\cos(E'+\bar{\omega})$$

展开：
$$= e[\cos\bar{\omega}\sin E'\cos\bar{\omega} + \cos\bar{\omega}\cos E'\sin\bar{\omega}]$$
$$\quad\; - e[\sin\bar{\omega}\cos E'\cos\bar{\omega} - \sin\bar{\omega}\sin E'\sin\bar{\omega}]$$

$$= e\sin E'(\cos^2\bar{\omega} + \sin^2\bar{\omega}) = e\sin E'$$

因此：

$$E - k\sin E + h\cos E = (E' + \bar{\omega}) - e\sin E' = (E' - e\sin E') + \bar{\omega} = M + \bar{\omega} = L \quad \checkmark$$

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
- $L$ 的值没有做 $\mod 2\pi$ 的归一化，以保留完整的圈数信息，便于与 VSOP 原始数据比较。
- 往返精度（`ELL → XYZ → ELL`）预期在双精度浮点的机器精度量级（$\sim 10^{-15}$），误差主要来自 `atan2` 的舍入。
