#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <stdio.h>

#include "HWAccelCUDA.cuh"


#if 0
}	// indent guard
#endif

void Legacy(int* in, int* out, int n)
{
	for (int i = 0; i < n; i++) {
		out[i] = in[i] * 2;
	}
}

__global__ void reduce0(double* g_idata, double* g_odata) {
	extern __shared__ double sdata[];
	// each thread loads one element from global to shared mem
	unsigned int tid = threadIdx.x;
	unsigned int i = blockIdx.x * blockDim.x + threadIdx.x;
	sdata[tid] = g_idata[i];
	__syncthreads();
	// do reduction in shared mem
	for (unsigned int s = blockDim.x / 2; s > 0; s >>= 1) {
		if (tid < s) {
			sdata[tid] += sdata[tid + s];
		}
		__syncthreads();
	}

	// write result for this block to global mem
	if (tid == 0) g_odata[blockIdx.x] = sdata[0];
}


__global__ void CalculateTerm(double* dAA, double* dBB, double* dSS, double* dCC, double* dRR, int n, double tj, double tit)
{
	int i = threadIdx.x;
	if (i < n) {
		double u = dAA[i] + (dBB[i] * tj);
		double su = sin(u);
		double cu = cos(u);
		dRR[i] = tit * (dSS[i] * su + dCC[i] * cu);
	}
}

double SUM(double* hIn, int n)
{
	double* dIn;
	double* dOut;
	double* hOut;
	int power = 0;
	int gridSize = n;
	while (true) {
		if (gridSize > 256) {
			power += 1;
			gridSize /= 256;
			continue;
		}
		break;
	}

	int blockSize = ((n / 256) + 1) * 256;
	gridSize = (n / 256) + 1;
	cudaMalloc((void**)&dIn, blockSize * sizeof(double));
	cudaMalloc((void**)&dOut, blockSize * sizeof(double));
	cudaMallocHost((void**)&hOut, blockSize * sizeof(double));
	cudaMemcpy(dIn, hIn, n * sizeof(double), cudaMemcpyHostToDevice);

	cudaMemset(dIn, 0, blockSize * sizeof(double));

	
	while (true)
	{
		reduce0 <<<gridSize, 256>>> (dIn, dOut);
		cudaMemset(dIn, 0, blockSize * sizeof(double));
		cudaMemcpy(dIn, dOut, gridSize * sizeof(double), cudaMemcpyDeviceToDevice);
		if (gridSize == 1) {
			break;
		}
		gridSize = gridSize / 256 + 1;
	}
	reduce0 <<<1, 256 >>> (dIn, dOut);
	cudaDeviceSynchronize();
	cudaMemcpy(hOut, dOut, gridSize * sizeof(double), cudaMemcpyDeviceToHost);
	double result = hOut[0];
	cudaFree(dIn);
	cudaFree(dOut);
	cudaFree(hOut);
	return result;
}

void CUDA(double* hAA, double* hBB,double* hSS,double* hCC, double* hRR, int n,double tj,double tit)
{
	int blocksize = (n / 256) + 1;
	int cudasize = blocksize * 256;

	double* dAA;
	double* dBB;
	double* dSS;
	double* dCC;
	double* dRR;

	cudaMalloc((void**)&dAA, cudasize * sizeof(double));
	cudaMalloc((void**)&dBB, cudasize * sizeof(double));
	cudaMalloc((void**)&dSS, cudasize * sizeof(double));
	cudaMalloc((void**)&dCC, cudasize * sizeof(double));
	cudaMalloc((void**)&dRR, cudasize * sizeof(double));


	cudaMemcpy(dAA, hAA, n * sizeof(double), cudaMemcpyHostToDevice);
	cudaMemcpy(dBB, hBB, n * sizeof(double), cudaMemcpyHostToDevice);
	cudaMemcpy(dSS, hSS, n * sizeof(double), cudaMemcpyHostToDevice);
	cudaMemcpy(dCC, hCC, n * sizeof(double), cudaMemcpyHostToDevice);
	
	cudaMemset(dRR, 0, cudasize * sizeof(double));

	CalculateTerm<<<blocksize, 256>>> (dAA,dBB,dSS,dCC,dRR,n,tj,tit);
	cudaDeviceSynchronize();

	cudaMemcpy(hRR, dRR, n * sizeof(double), cudaMemcpyDeviceToHost);

	cudaFree(dAA);
	cudaFree(dBB);
	cudaFree(dSS);
	cudaFree(dCC);
	cudaFree(dRR);
}
