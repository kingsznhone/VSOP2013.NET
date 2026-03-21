# ELL → XYZ 正运算：数学原理与公式推导

作者 : Claude Opus 4.6

## 概述

本文档解析 `ELLtoXYZ` 函数的数学原理。该函数源自 VSOP2013 原始 Fortran 代码，使用复数运算将修正交点根数 $(a, L, k, h, q, p)$ 转换为日心笛卡尔坐标 $(x, y, z, \dot{x}, \dot{y}, \dot{z})$。

原代码注释写道 *"This is kind of magic that I will never understand"*——本文档旨在彻底破解这个"魔法"。

---

## 1. 输入参数

| 变量 | 含义 | 定义 |
|------|------|------|
| $a$ | 半长轴 | — |
| $L$ | 平经度 | $L = M + \bar{\omega}$，其中 $M$ 是平近点角 |
| $k$ | 偏心率分量 | $k = e\cos\bar{\omega}$ |
| $h$ | 偏心率分量 | $h = e\sin\bar{\omega}$ |
| $q$ | 倾角分量 | $q = \sin(i/2)\cos\Omega$ |
| $p$ | 倾角分量 | $p = \sin(i/2)\sin\Omega$ |

其中 $\bar{\omega} = \Omega + \omega$ 是经度近日点。

---

## 2. 初始化阶段

### 2.1 辅助常数

```csharp
rgm = Math.Sqrt(gmp[(int)body] + gmsol);
```

$$\texttt{rgm} = \sqrt{\mu} = \sqrt{GM_\text{planet} + GM_\odot}$$

### 2.2 几何辅助量

```csharp
xfi = Math.Sqrt(1.0d - (k * k) - (h * h));
xki = Math.Sqrt(1.0d - (q * q) - (p * p));
u = 1.0d / (1.0d + xfi);
```

| 代码变量 | 数学含义 | 公式 |
|----------|---------|------|
| `xfi` ($\phi$) | $\sqrt{1-e^2}$ | 因为 $k^2+h^2 = e^2$ |
| `xki` ($\chi$) | $\cos(i/2)$ | 因为 $q^2+p^2 = \sin^2(i/2)$ |
| `u` | 辅助因子 | $u = \dfrac{1}{1+\phi}$ |

其中 $\phi = \sqrt{1-e^2}$ 是偏心率的"补量"，在椭圆轨道中 $0 < \phi \le 1$。

### 2.3 偏心率的复数表示

```csharp
z = new Complex(k, h);
ex = z.Magnitude;
z1 = Complex.Conjugate(z);
```

$$\mathbf{z} = k + ih = e \cdot e^{i\bar{\omega}}$$

- $|\mathbf{z}| = e$（偏心率）
- $\arg(\mathbf{z}) = \bar{\omega}$（经度近日点）
- $\bar{\mathbf{z}} = k - ih$（共轭）

> **核心思想**：将偏心率和近日点经度编码为一个复数，后续所有角度变换都变成复数乘法。

---

## 3. 解交点形式的开普勒方程

### 3.1 问题定义

经典开普勒方程为 $M = E' - e\sin E'$。在交点根数下，平经度 $L = M + \bar{\omega}$，偏经度 $E = E' + \bar{\omega}$，方程变为：

$$L = E - k\sin E + h\cos E$$

等价地（见推导）：

$$L = E - \text{Im}(\bar{\mathbf{z}} \cdot e^{iE})$$

这是因为：

$$\bar{\mathbf{z}} \cdot e^{iE} = (k-ih)(\cos E + i\sin E) = (k\cos E + h\sin E) + i(k\sin E - h\cos E)$$

取虚部：$\text{Im}(\bar{\mathbf{z}} \cdot e^{iE}) = k\sin E - h\cos E$

因此 $L = E - (k\sin E - h\cos E) = E - k\sin E + h\cos E$。 ✓

### 3.2 初始猜测

```csharp
gl = ell[1] % (Math.Tau);
gm = gl - Math.Atan2(h, k);
e = gl + (ex - 0.125d * ex3) * Math.Sin(gm)
    + 0.5d * ex2 * Math.Sin(2.0d * gm)
    + 0.375d * ex3 * Math.Sin(3.0d * gm);
```

| 代码变量 | 数学含义 |
|----------|---------|
| `gl` | $\lambda = L \bmod 2\pi$（归一化平经度）|
| `gm` | $M = \lambda - \bar{\omega}$（平近点角）|
| `e`（初值）| $E$ 的初始猜测 |

初始猜测使用了开普勒方程的三阶级数展开：

$$E_0 \approx \lambda + \left(e - \frac{e^3}{8}\right)\sin M + \frac{e^2}{2}\sin 2M + \frac{3e^3}{8}\sin 3M$$

这是经典的 $E = M + \sum c_n \sin(nM)$ 展开，对于太阳系行星（$e < 0.25$），仅需 3 项就能给出非常好的初始值。

> **注意**：代码中变量名 `e` 在此处被复用为偏经度 $E$ 的迭代值（而非偏心率），偏心率始终使用 `ex`。

### 3.3 Newton-Raphson 迭代

```csharp
while (true)
{
    z2 = new Complex(0d, e);        // z2 = iE
    zteta = Complex.Exp(z2);        // ζ = e^{iE} = cos(E) + i·sin(E)
    z3 = z1 * zteta;               // z̄·ζ = (k-ih)(cos E + i sin E)
    dl = gl - e + z3.Imaginary;    // δ = λ - E + Im(z̄·ζ)
    rsa = 1.0d - z3.Real;          // 1 - Re(z̄·ζ)
    e += dl / rsa;                 // E ← E + δ/(1 - Re(z̄·ζ))
    if (Math.Abs(dl) < 1e-15) break;
}
```

#### 方程与残差

要解的方程是 $f(E) = 0$，其中：

$$f(E) = \lambda - E + \text{Im}(\bar{\mathbf{z}} \cdot e^{iE}) = 0$$

这就是 $\lambda = E - \text{Im}(\bar{\mathbf{z}} \cdot e^{iE})$ 的移项形式。

残差：$\delta = \lambda - E + \text{Im}(\bar{\mathbf{z}} \cdot e^{iE})$（代码中的 `dl`）

#### 导数

$$f'(E) = -1 + \text{Re}(\bar{\mathbf{z}} \cdot i \cdot e^{iE}) = -1 + \text{Re}(i \cdot \bar{\mathbf{z}} \cdot e^{iE})$$

由于 $\bar{\mathbf{z}} \cdot e^{iE}$ 的实部为 $k\cos E + h\sin E$，乘以 $i$ 后实部变为 $-(k\sin E - h\cos E)$... 但更直接地：

$$\frac{d}{dE}\text{Im}(\bar{\mathbf{z}} \cdot e^{iE}) = \text{Re}(\bar{\mathbf{z}} \cdot e^{iE})$$

> 因为 $\frac{d}{dE} e^{iE} = ie^{iE}$，取虚部的导数等于实部。

所以：$f'(E) = -1 + \text{Re}(\bar{\mathbf{z}} \cdot e^{iE})$

Newton 更新公式为：

$$E \leftarrow E - \frac{f(E)}{f'(E)} = E + \frac{\delta}{1 - \text{Re}(\bar{\mathbf{z}} \cdot e^{iE})}$$

代码中 `rsa` = $1 - \text{Re}(\bar{\mathbf{z}} \cdot e^{iE})$ = $-f'(E)$，所以 `e += dl / rsa` 就是 Newton 步。

#### 物理意义

$\text{Re}(\bar{\mathbf{z}} \cdot e^{iE}) = e\cos(E - \bar{\omega}) = e\cos E'$

因此 `rsa` = $1 - e\cos E'$ = $r/a$（归一化距离），这是开普勒方程的经典几何量。

---

## 4. 轨道面内的位置（复数形式）

### 4.1 计算 `zto`（轨道面内复坐标 ÷ 距离）

```csharp
z1 = u * z * z3.Imaginary;
z2 = new Complex(z1.Imaginary, -z1.Real);
zto = (-z + zteta + z2) / rsa;
```

> **注意**：这里 `z1` 被重新赋值（不再是共轭），`z3` 保留了迭代结束时的值。

逐行解读：

1. **`z3.Imaginary`**：
   迭代结束时 $\mathbf{z}_3 = \bar{\mathbf{z}} \cdot \zeta$，其虚部为 $k\sin E - h\cos E = -e\sin E'$（带符号的偏近点角正弦）。但实际上更精确地说，$\text{Im}(\bar{\mathbf{z}} \cdot e^{iE}) = k\sin E - h\cos E$。

   用 $s$ 表示 `z3.Imaginary`。

2. **`z1 = u * z * s`**：

   $$\mathbf{z}_1 = \frac{s}{1+\phi} \cdot (k + ih)$$

   这是一个与偏心率和偏近点角正弦相关的修正项。

3. **`z2 = new Complex(z1.Imaginary, -z1.Real)`**：

   这等价于 $\mathbf{z}_2 = -i \cdot \mathbf{z}_1$（乘以 $-i$ 等于将实虚部交换并取负实部变虚部）。

   验证：若 $\mathbf{z}_1 = a_1 + ib_1$，则 $-i\mathbf{z}_1 = -i(a_1+ib_1) = b_1 - ia_1$。代码给出 $(b_1, -a_1)$。✓

4. **`zto = (-z + zteta + z2) / rsa`**：

   $$\mathbf{Z}_\theta = \frac{-\mathbf{z} + e^{iE} - i \cdot u \cdot \mathbf{z} \cdot s}{r/a}$$

#### 数学推导

设 $\zeta = e^{iE}$，$\bar{\mathbf{z}} = k - ih$，$s = \text{Im}(\bar{\mathbf{z}}\zeta)$。

经典的交点根数位置公式，在轨道面的复坐标为：

$$\mathbf{Z}_\theta = \cos F + i\sin F = \frac{-\mathbf{z} + \zeta + \frac{s}{1+\phi}(-i\mathbf{z})}{1 - \text{Re}(\bar{\mathbf{z}}\zeta)}$$

其中 $F$ 是真经度。分子中：
- $-\mathbf{z}$：偏心率修正的偏移
- $\zeta = e^{iE}$：偏经度旋转
- $-i\mathbf{z} \cdot s/(1+\phi)$：二阶修正项（与偏心率平方相关）

`zto` 的实部和虚部分别对应轨道面内的归一化坐标 $(\cos F, \sin F)$，但实际上并不精确等于 $\cos F$ 和 $\sin F$——它们已经包含了径向距离的缩放（除以 $r/a$），所以实际的轨道面内位置为：

$$x_\text{orb} = a \cdot \text{rsa} \cdot \text{Re}(\mathbf{Z}_\theta), \quad y_\text{orb} = a \cdot \text{rsa} \cdot \text{Im}(\mathbf{Z}_\theta)$$

### 4.2 距离

```csharp
xr = a * rsa;
```

$$r = a \cdot (1 - e\cos E') = a \cdot \texttt{rsa}$$

---

## 5. 从轨道面到三维空间（旋转矩阵）

### 5.1 位置

```csharp
xm = p * zto.Real - q * zto.Imaginary;

xyz[0] = xr * (zto.Real - 2.0d * p * xm);
xyz[1] = xr * (zto.Imaginary + 2.0d * q * xm);
xyz[2] = -2.0d * xr * xki * xm;
```

设 $X = \text{Re}(\mathbf{Z}_\theta)$，$Y = \text{Im}(\mathbf{Z}_\theta)$，$m = pX - qY$，则：

$$\vec{r} = r \begin{pmatrix} X - 2pm \\ Y + 2qm \\ -2\chi m \end{pmatrix}$$

展开 $m = pX - qY$：

$$x = r\left[(1-2p^2)X + 2pqY\right]$$
$$y = r\left[2pqX + (1-2q^2)Y\right]$$
$$z = r\left[-2p\chi X + 2q\chi Y\right]$$

写成矩阵形式：

$$\vec{r} = r \begin{pmatrix} 1-2p^2 & 2pq \\ 2pq & 1-2q^2 \\ -2p\chi & 2q\chi \end{pmatrix} \begin{pmatrix} X \\ Y \end{pmatrix}$$

矩阵的两列就是轨道面基向量 $\hat{e}_1$ 和 $\hat{e}_2$：

$$\hat{e}_1 = \begin{pmatrix} 1 - 2p^2 \\ 2pq \\ -2p\chi \end{pmatrix}, \quad \hat{e}_2 = \begin{pmatrix} 2pq \\ 1 - 2q^2 \\ 2q\chi \end{pmatrix}$$

#### 旋转矩阵的来源

这个旋转矩阵等价于经典的 $R_z(-\Omega) R_x(-i) R_z(-\omega)$，但使用 $q, p, \chi$ 的参数化避免了三角函数运算。

具体地，利用 $q = \sin(i/2)\cos\Omega$，$p = \sin(i/2)\sin\Omega$，$\chi = \cos(i/2)$：

- $1 - 2p^2 = 1 - 2\sin^2(i/2)\sin^2\Omega = \cos^2\Omega + \cos i \cdot \sin^2\Omega$

这些恒等式与 Rodrigues 旋转公式一致。

> **重要特征**：这里只有 $\hat{e}_1, \hat{e}_2$ 两个列向量（而非完整的 3×3 旋转矩阵），因为轨道面内的运动只有两个自由度。

### 5.2 速度

```csharp
xms = a * (h + zto.Imaginary) / xfi;
xmc = a * (k + zto.Real) / xfi;
xn = rgm / (a * Math.Sqrt(a));

xyz[3] = xn * ((2.0d * p * p - 1.0d) * xms + 2.0d * p * q * xmc);
xyz[4] = xn * ((1.0d - 2.0d * q * q) * xmc - 2.0d * p * q * xms);
xyz[5] = xn * 2.0d * xki * (p * xms + q * xmc);
```

#### 平均角速度

$$n = \frac{\sqrt{\mu}}{a^{3/2}} = \frac{\texttt{rgm}}{a\sqrt{a}}$$

这是开普勒第三定律：$n^2 a^3 = \mu$。

#### 轨道面内速度分量

| 代码变量 | 数学含义 | 公式 |
|----------|---------|------|
| `xms` | $S = a(h + Y)/\phi$ | 轨道面内的 $\hat{e}_2$ 方向速度因子 |
| `xmc` | $C = a(k + X)/\phi$ | 轨道面内的 $\hat{e}_1$ 方向速度因子 |

这些量来自对轨道面内位置关于时间的微分，利用了开普勒方程的时间导数关系。

在经典轨道力学中，轨道面内的速度为：

$$\dot{x}_\text{orb} = -na\sin E' / (1-e\cos E'), \quad \dot{y}_\text{orb} = na\phi\cos E' / (1-e\cos E')$$

但在交点根数的复数表达中，速度的 $\hat{e}_1$ 和 $\hat{e}_2$ 分量被编码为 $C$ 和 $S$。

#### 旋转到三维空间

$$\vec{v} = n \begin{pmatrix} (2p^2-1)S + 2pqC \\ (1-2q^2)C - 2pqS \\ 2\chi(pS + qC) \end{pmatrix}$$

注意速度旋转矩阵与位置的略有不同——这里 $\hat{e}_1$ 列前面的符号翻转了（$(2p^2-1)$ 而非 $(1-2p^2)$），相当于对 $S$ 分量（$\hat{e}_2$ 方向）取了负号后再旋转。这对应于轨道面内速度方向与位置方向的 $90°$ 相位差。

---

## 6. 算法流程总结

```
输入: a, L, k, h, q, p
  │
  ├─ 计算辅助量: φ, χ, e, ω̄
  │
  ├─ 解交点开普勒方程 (Newton迭代):
  │   L = E - k·sin(E) + h·cos(E)  →  求 E
  │
  ├─ 计算轨道面内复坐标 Z_θ = X + iY:
  │   利用 e^{iE} 和复数偏心率向量
  │
  ├─ 计算距离: r = a·(1 - Re(z̄·e^{iE}))
  │
  ├─ 旋转到三维 (位置):
  │   [x,y,z] = r · [ê₁, ê₂] · [X, Y]ᵀ
  │
  └─ 旋转到三维 (速度):
      [ẋ,ẏ,ż] = n · 旋转矩阵 · [C, S]ᵀ
```

---

## 7. 复数运算的优雅之处

整个算法最巧妙的设计在于，将偏心率 $(k, h)$ 和角度 $E$ 统一编码到复平面上：

1. **偏心率向量** $\mathbf{z} = k + ih = e \cdot e^{i\bar{\omega}}$
2. **偏经度旋转** $\zeta = e^{iE}$
3. **开普勒方程** 变成 $\lambda = E - \text{Im}(\bar{\mathbf{z}}\zeta)$，Newton 迭代只需一次复数乘法
4. **轨道面内位置** 直接由 $\zeta$ 和 $\mathbf{z}$ 的代数组合给出

这种写法避免了显式计算 $\bar{\omega}$、$E'$、$\nu$ 等中间角度，减少了三角函数调用，同时消除了 $\bar{\omega} = 0$ 或 $e = 0$ 时的奇异性问题。

---

## 8. 代码变量与数学符号对照表

| 代码变量 | 数学符号 | 含义 |
|----------|---------|------|
| `a` | $a$ | 半长轴 |
| `gl` | $\lambda$ | 归一化平经度 |
| `k`, `h` | $k$, $h$ | 偏心率分量 |
| `q`, `p` | $q$, $p$ | 倾角分量 |
| `z` | $\mathbf{z} = k+ih$ | 复偏心率 |
| `ex` | $e = \|\mathbf{z}\|$ | 偏心率 |
| `xfi` | $\phi = \sqrt{1-e^2}$ | 偏心率补量 |
| `xki` | $\chi = \cos(i/2)$ | 倾角余弦半角 |
| `u` | $u = 1/(1+\phi)$ | 辅助因子 |
| `gm` | $M$ | 平近点角 |
| `e`（迭代后）| $E$ | 偏经度 |
| `zteta` | $\zeta = e^{iE}$ | 偏经度旋转子 |
| `rsa` | $r/a = 1 - e\cos E'$ | 归一化距离 |
| `zto` | $\mathbf{Z}_\theta$ | 轨道面内归一化复坐标 |
| `xm` | $m = pX - qY$ | 面外分量因子 |
| `xr` | $r = a \cdot \texttt{rsa}$ | 日心距离 |
| `xms` | $S = a(h+Y)/\phi$ | 速度 $\hat{e}_2$ 分量因子 |
| `xmc` | $C = a(k+X)/\phi$ | 速度 $\hat{e}_1$ 分量因子 |
| `xn` | $n = \sqrt{\mu}/a^{3/2}$ | 平均角速度 |
| `rgm` | $\sqrt{\mu}$ | 引力参数平方根 |
