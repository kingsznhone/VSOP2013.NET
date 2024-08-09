#pragma once

#ifdef DLLEXPORT// DLLEXPORT
#define DLL_EXPORT __declspec(dllexport)
#else
#define DLL_EXPORT __declspec(dllimport)
#endif

struct Term
{
    double ss;
    double cc;
    double aa;
    double bb;
};

#ifdef __cplusplus
extern "C" {
#endif

    DLL_EXPORT  double Substitution(struct Term* terms, int length, double tj, double tit);

#ifdef __cplusplus
}
#endif