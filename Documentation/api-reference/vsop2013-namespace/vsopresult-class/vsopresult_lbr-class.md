# VSOPResult\_LBR Class

## Definition

VSOP2013 calculate result represent in spherical coordinate system.

```csharp
public class VSOPResult_LBR : VSOPResult
```

## Constructors <a href="#constructors" id="constructors"></a>

```csharp
public VSOPResult_LBR(VSOPResult_ELL result)
```

Create a new spherical result from elliptic result.



**Parameters**

`result` [VSOPResult\_ELL](vsopresult\_ell-class.md)&#x20;

Elliptic result



***

```csharp
public VSOPResult_LBR(VSOPResult_XYZ result)
```

Create a new spherical result from cartesian result.



**Parameters**

`result` [VSOPResult\_XYZ](vsopresult\_xyz-class.md)

Cartesian result



***

## Properties <a href="#properties" id="properties"></a>

`double l {get;}`

longitude (rd)



`double b {get;}`

latitude (rd)



`double r {get;}`

radius (au)



`double dl {get;}`

longitude velocity (rd/day)



`double db {get;}`

latitude velocity (rd/day)



`double dr {get;}`

radius velocity (au/day)

***

## Methods

```csharp
public VSOPResult_ELL ToELL()
```

Convert this result to elliptic coordinate.



<pre class="language-csharp"><code class="lang-csharp"><strong>public VSOPResult_XYZ ToXYZ()
</strong></code></pre>

Convert this result to cartesian coordinate.
