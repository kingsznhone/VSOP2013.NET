#pragma once
#ifdef  DLL_EXPORT
/*Enabled as "export" while compiling the dll project*/
#define DLLEXPORT __declspec(dllexport)  
#else
/*Enabled as "import" in the Client side for using already created dll file*/
#define DLLEXPORT __declspec(dllimport)  
#endif

#ifdef DLL_EXPORT
extern "C" {
    DLLEXPORT void Legacy(int* in, int* out, int n);
    DLLEXPORT void CUDA(double* AA,double* BB,double* SS ,double*CC, double* RR,int n, double tj,double tit);
    DLLEXPORT double SUM(double* hIn, int n);
}
#else
extern "C" {
    DLLEXPORT void Legacy(int* in, int* out, int n);
    DLLEXPORT void CUDA(double* AA, double* BB, double* SS, double* CC, double* RR, int n, double tj, double tit);
    DLLEXPORT double SUM(double* hIn, int n);

}
#endif

