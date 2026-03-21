# Header Records

#### Fortran Format:

&#x20;         read (ifile,1001) ip,iv,it,nt

1001 format (9x,3i3,i7)



#### Specifications:

* ip : planete index (integer)
* iv : variable index (integer)
* it : time power a (integer)
* nt : number of terms in series (integer)

#### Planet index (ip):

1. Mercury
2. Venus
3. Earth-Moon Barycenter
4. Mars
5. Jupiter
6. Saturn
7. Uranus
8. Neptune
9. Pluto

#### Variable index (iv):

1. a = semi-major axis (ua)
2. l = mean longitude (radian)
3. k = e cos **ω**
4. h = e sin **ω**
5. q = sin(i/2) cos Ω
6. p = sin(i/2) sin Ω

#### Time power α (it):

* it=0 : Periodic terms
* it>0 : Poisson terms
