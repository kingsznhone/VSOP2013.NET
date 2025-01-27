# Data Structs

## PlanetTable

```csharp
public struct PlanetTable
```

### Fields <a href="#fields" id="fields"></a>

[`VSOPBody`](enums.md#fields)  body

Planet Enum



[`VariableTable`](data-structs.md#variabletable)`[]`  variables

All 6 variable data stored.

***

## VariableTable

```csharp
public struct VariableTable
```

### Fields <a href="#fields" id="fields"></a>

[`VSOPBody`](enums.md#fields) body

Planet Enum



[`VSOPVariable`](enums.md#fields-2) Variable

Which variable of this table.



[`PowerTable`](data-structs.md#powertable)`[]` PowerTables

All power table stored.

***

## PowerTable

```csharp
public struct PowerTable
```

### Fields <a href="#fields" id="fields"></a>

[`VSOPBody`](enums.md#fields) body

Planet Enum



[`VSOPVariable`](enums.md#fields-2) Variable

Which variable of this table.



`int Power`

Power of this table.



`int` TermsCount

Total terms count.



[`Term`](data-structs.md#term)`[]` Terms

All terms stored.



***

## Term

```csharp
public struct Term
```

### Fields <a href="#fields" id="fields"></a>

`double` ss



`double` cc



`double` aa



`double` bb



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
