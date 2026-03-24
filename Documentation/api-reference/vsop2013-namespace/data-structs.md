# Data Structs

## PlanetTable

```csharp
public class PlanetTable
```

### Fields <a href="#fields" id="fields"></a>

`body` [VSOPBody](enums.md#fields)

Planet Enum



`variables`  Dictionary<[VSOPVariable](enums.md#fields-2), [VariableTable](data-structs.md#variabletable)>

All 6 variable data stored.

***

## VariableTable

```csharp
public class VariableTable
```

### Fields <a href="#fields" id="fields"></a>

`body` [VSOPBody](enums.md#fields)

Planet Enum



`Variable` [VSOPVariable](enums.md#fields-2)

Which variable of this table.



`PowerTables` [PowerTable\[\]](data-structs.md#powertable)

All power table stored.

***

## PowerTable

```csharp
public class PowerTable
```

### Fields <a href="#fields" id="fields"></a>

`body` [VSOPBody](enums.md#fields)

Planet Enum



`Variable` [VSOPVariable](enums.md#fields-2)

Which variable of this table.



`Power` int

Power of this table.



`TermsCount` int

Total terms count.



`Terms` [Term\[\]](data-structs.md#term)

All terms stored.

***

## Term

```csharp
public struct Term
```

### Fields <a href="#fields" id="fields"></a>

`ss`  double



`cc`  double



`aa`  double



`bb`  double



***

## Header

```csharp
public struct Header
```

### Fields <a href="#fields" id="fields"></a>

`int` ip

Planet index

`int` iv

Variable index

`int` it

index of terms

`int` nt

total terms count
