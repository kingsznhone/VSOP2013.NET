# VSOPResult\_ELL Class

## Definition

VSOP2013 calculate result represent in elliptic coordinate system.

```csharp
public class VSOPResult_ELL : VSOPResult
```

## Constructors <a href="#constructors" id="constructors"></a>

```csharp
public VSOPResult_ELL(VSOPBody body, VSOPTime time, double[] ell)
```

Create a new elliptic result from calculate result.



**Parameters**

`body` [VSOPBody](../enums.md#fields)

Planet



`time` [VSOPTime](../vsoptime-class.md)

Time wrapper for VSOP



`ell` double\[]

Raw result data from calculator.



```csharp
public VSOPResult_ELL(VSOPResult_XYZ result)
```

Create a new elliptic result from cartesian result.



**Parameters**

`result` [VSOPResult\_XYZ ](vsopresult_xyz-class.md)

Cartesian result



```csharp
public VSOPResult_ELL(VSOPResult_LBR result)
```

Create a new elliptic result from spherical result.



**Parameters**

`result` [VSOPResult\_LBR](vsopresult_lbr-class.md)

Spherical result



***

## Properties <a href="#properties" id="properties"></a>

`public double a {get;}`

Semi-major axis (au)



`public double l {get;}`

Mean longitude (rd)



`public double k {get;}`

e\*cos(pi) (rd)



`public double h {get;}`

e\*sin(pi) (rd)



`public double q {get;}`&#x20;

sin(i/2)\*cos(omega) (rd)



`public double p {get;}`

sin(i/2)\*sin(omega) (rd)

***

## Methods

```csharp
public VSOPResult_XYZ ToXYZ()
```

Convert this result to cartisian coordinate.



<pre class="language-csharp"><code class="lang-csharp"><strong>public VSOPResult_LBR ToLBR()
</strong></code></pre>

Convert this result to spherical coordinate.

