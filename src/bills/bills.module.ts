import { Module } from '@nestjs/common';
import { BillsController } from './bills.controller';
import { BillsService } from './bills.service';
import { PrismaService } from 'src/prisma/prisma.service';

@Module({
  providers: [BillsService, PrismaService],
  controllers: [BillsController],
  exports: [BillsService],
})
export class BillsModule {}
