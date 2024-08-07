#include "NativeAccelerator.h"
#include "math.h"

double Substitution(struct Term* terms, int length, double tj, double tit) {
    double result = 0;
    for (int n = 0; n < length; n++) {
        double u = terms[n].aa + terms[n].bb * tj;
        double su = sin(u);
        double cu = cos(u);
        result += tit * (terms[n].ss * su + terms[n].cc * cu);
    }
    return result;
}