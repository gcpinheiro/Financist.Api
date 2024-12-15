import {
  Injectable,
  ConflictException,
  UnauthorizedException,
} from '@nestjs/common';
import { CreateUserDto } from './dto/create-user.dto';
import { PrismaService } from 'src/prisma/prisma.service';
import * as bcrypt from 'bcryptjs';

@Injectable()
export class UsersService {
  constructor(private readonly prisma: PrismaService) {}

  async createUser(dto: CreateUserDto) {
    const { name, email, password } = dto;

    const userAlreadyExists = await this.prisma.user.findUnique({
      where: { email },
    });

    if (userAlreadyExists) {
      throw new ConflictException('User already exists');
    }

    // Criptografar a senha
    const hashedPassword = await bcrypt.hash(password, 10);

    return this.prisma.user.create({
      data: {
        name,
        email,
        password: hashedPassword,
      },
    });
  }

  async getUser(email: string) {
    const userFound = await this.prisma.user.findUnique({ where: { email } });

    if (userFound) {
      return {
        user: userFound,
      };
    } else {
      throw new UnauthorizedException('Nenhum usuário encontrado');
    }
  }
}
