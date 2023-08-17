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
    DLLEXPORT float CUDA(float* AABBSSCC, float* RR,int n, float tj, float tit);
    DLLEXPORT void Init();
}
#else
extern "C" {
    DLLEXPORT float CUDA(float* AABBSSCC, float* RR, int n, float tj, float tit);
    DLLEXPORT void Init();
}
#endif

