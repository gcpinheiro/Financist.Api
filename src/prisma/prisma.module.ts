import { Global, Module } from '@nestjs/common';
import { PrismaService } from './prisma.service';

@Global() // Torna o módulo global para evitar importar repetidamente
@Module({
  providers: [PrismaService],
  exports: [PrismaService],
})
export class PrismaModule {}
