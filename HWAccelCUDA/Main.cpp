/*
* Note:
* To switch output type (dll/exe), Project Configuration Properties -> Configuration Type
* *.exe for unit test
* *.dll for dll library
*/
#include "stdio.h"
#include "stdlib.h"

#include "HWAccelCUDA.cuh"


bool unitTest(int n)
{
	float* in = new float[4*n];
	float* out = new float[n];

	for (int i = 0; i < n*4; i++) in[i] = rand() % 100;

	
	Init();

	CUDA(in, out, 4, 10, 20);

	delete in;




	return true;
}

int main()
{
	if (unitTest(10)) {
		printf("OK\n");
	}
	else {
		printf("ERROR\n");
	}


	return 0;
}
