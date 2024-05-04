# VSOPResult\_XYZ Class

## Definition

VSOP2013 calculate result represent in cartesian coordinate system.

```csharp
public class VSOPResult_XYZ : VSOPResult
```

## Constructors <a href="#constructors" id="constructors"></a>

```csharp
public VSOPResult_XYZ(VSOPResult_ELL result)
```

Create a new cartesian result from elliptic  result.



**Parameters**

`result` [VSOPResult\_ELL ](vsopresult\_ell-class.md)

Elliptic result



***

```csharp
public VSOPResult_XYZ(VSOPResult_LBR result)
```

Create a new cartesian result from spherical result.



**Parameters**

`result`[ VSOPResult\_LBR](vsopresult\_lbr-class.md)

Spherical result



***

## Properties <a href="#properties" id="properties"></a>

`double x {get;}`

Position x (au)



`double y {get;}`

Position y (au)



`double z {get;}`

Position z (au)



`double dx {get;}`

Velocity x (au/day)



`double dy {get;}`

Velocity y (au/day)



`double dz {get;}`

Velocity z (au/day)

***

## Methods

```csharp
public VSOPResult_ELL ToELL()
```

Convert this result to elliptic coordinate.



<pre class="language-csharp"><code class="lang-csharp"><strong>public VSOPResult_LBR ToLBR()
</strong></code></pre>

Convert this result to spherical coordinate.
