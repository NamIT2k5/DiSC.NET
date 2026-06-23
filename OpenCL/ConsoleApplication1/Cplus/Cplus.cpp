// Cplus.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
struct KK
{
	int i;
	char c;
};
void helloWorld2(KK* message, int messageSize) {
	for (int i = 0; i < messageSize; i++) {
		printf("%d:%c\t", message[i].i,message[i].c);
	}
}

int _tmain(int argc, _TCHAR* argv[])
{
	const int n = 10;
	KK kk[n];
	for (int i = 0; i < n; i++)
	{
		kk[i].i = i;
		kk[i].c = 'a'+i;
	}
	helloWorld2(kk, n);
	getchar();
	return 0;
}

