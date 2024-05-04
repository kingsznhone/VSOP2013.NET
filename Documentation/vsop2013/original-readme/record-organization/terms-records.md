# Terms Records

The "TERMS" records are put in the file according to the decreasing values of the sum of absolute values of the coefficients S and C: |S|+|C|.

Each "TERMS" record contains respectively: the rank of the term in the series and the quantities a(i) (i=1,17), S and C

#### Fortran format:&#x20;

&#x20;           read (ifile,1002) num,(iphi(i),i=1,17),c1,ie1,c2,ie2&#x20;

1002 format (i5,1x,4i3,1x,5i3,1x,4i4,1x,i6,1x,3i3,2(f20.16,1x,i3))

#### Specifications:

num : rank of the terms in the series (integer)

iphi : 17 numerical coefficients a(i) (i=1,17) (integer)

c1, ie1 : coefficient S, mantissa and exponent (real\*8 and integer)

c2, ie2 : coefficient C, mantissa and exponent (real\*8 and integer)

Units of the coefficients: au for a, radian for λ, without unit for the other variables k, h, q, p



