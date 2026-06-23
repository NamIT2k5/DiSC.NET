kernel void helloWorld(global read_only int* message, int messageSize) {
	
  for (int i = 0; i < messageSize; i++) {
    printf("%d", message[i]);
	
  }
  size_t id = get_global_id(0);
  printf("\n%d", id);
}

