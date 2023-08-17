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
	int* in = new int[n];
	int* out1 = new int[n];
	int* out2 = new int[n];

	for (int i = 0; i < n; i++) in[i] = rand() % 100;

	Legacy(in, out1, n);
	int i;
	for (i = 0; i < n; i++) {
		if (out1[i] != out2[i]) {
			printf("error at index = %d\n", i);
			break;
		}
	}

	delete in;
	delete out1;
	delete out2;

	if (i == n) return true;
	return false;
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
