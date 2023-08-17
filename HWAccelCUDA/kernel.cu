#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <stdio.h>

#include "HWAccelCUDA.cuh"
#include <numeric>

#if 0
}	// indent guard
#endif

static float* dAABBSSCC;
static float* dRR;
__global__ void CalculateTerm(float* dAABBSSCC, float* dRR, int n, float tj, float tit)
{
	int i = blockDim.x * blockIdx.x + threadIdx.x;
	if (i < n) {
		float aa = dAABBSSCC[i];
		float bb = dAABBSSCC[n+i];
		float ss = dAABBSSCC[2*n + i];
		float cc = dAABBSSCC[3*n + i];
		float u = aa + (bb * tj);
		float su = sin(u);
		float cu = cos(u);
		dRR[i] = tit * (ss * su + cc * cu);
	}
}

void Init() {
	cudaMalloc((void**)&dAABBSSCC, 4 * 32768 * sizeof(float));
	cudaMalloc((void**)&dRR, 32768 * sizeof(float));
}



float CUDA(float* hAABBSSCC, float* hRR, int n, float tj, float tit)
{
	int blocksize = (n / 256) + 1;
	int cudasize = blocksize * 256;

	//cudaMalloc((void**)&dAABBSSCC, 4 * cudasize * sizeof(float));
	//cudaMalloc((void**)&dRR, n * sizeof(float));

	cudaMemcpy(dAABBSSCC, hAABBSSCC, 4* n * sizeof(float), cudaMemcpyHostToDevice);

	CalculateTerm<<<blocksize, 256>>> (dAABBSSCC,dRR,n,tj,tit);
	cudaDeviceSynchronize();

	cudaMemcpy(hRR, dRR, n * sizeof(float), cudaMemcpyDeviceToHost);

	/*cudaFree(dAABBSSCC);
	cudaFree(dRR);*/

	float sum = 0;
	for (int i = 0;i < n; i++) {
		sum += hRR[i];
	}

	return sum;
}
